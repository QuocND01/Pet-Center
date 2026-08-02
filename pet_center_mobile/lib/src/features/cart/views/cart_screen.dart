import 'package:flutter/material.dart';
import '../../../constants/app_colors.dart';
import '../../../models/cart_model.dart';
import '../../../services/api_service.dart';
import '../../../widgets/custom_button.dart';
import '../../checkout/views/checkout_screen.dart';

class CartScreen extends StatefulWidget {
  const CartScreen({super.key});

  @override
  State<CartScreen> createState() => _CartScreenState();
}

class _CartScreenState extends State<CartScreen> {
  final ApiService _apiService = ApiService();
  late Future<CartResponseModel> _cartFuture;
  CartResponseModel? _currentCart;
  bool _isUpdating = false;

  @override
  void initState() {
    super.initState();
    _loadCart();
  }

  void _loadCart() {
    if (_apiService.customerId == null || _apiService.customerId!.isEmpty) {
      return;
    }
    setState(() {
      _cartFuture = _apiService.getCart(_apiService.customerId!).then((cart) {
        _currentCart = cart;
        return cart;
      });
    });
  }

  List<CartDetailModel> get _selectedItems {
    if (_currentCart == null) return [];
    return _currentCart!.cartDetails.where((item) => item.isSelected).toList();
  }

  bool get _isAllSelected {
    if (_currentCart == null || _currentCart!.cartDetails.isEmpty) return false;
    return _currentCart!.cartDetails.every((item) => item.isSelected);
  }

  void _toggleSelectAll(bool? value) {
    if (_currentCart == null) return;
    setState(() {
      for (var detail in _currentCart!.cartDetails) {
        detail.isSelected = value ?? false;
      }
    });
  }

  double _calculateTotal() {
    double total = 0.0;
    for (var detail in _selectedItems) {
      if (detail.product != null) {
        total += detail.product!.productPrice * detail.quantity;
      }
    }
    return total;
  }

  void _updateQuantity(CartDetailModel detail, int newQty) async {
    if (newQty < 1 || _isUpdating) return;
    setState(() {
      _isUpdating = true;
    });

    try {
      final success = await _apiService.updateCartQuantity(detail.cartDetailId, newQty);
      if (success) {
        setState(() {
          detail.quantity = newQty;
        });
      } else {
        _showError('Failed to update quantity.');
      }
    } catch (e) {
      _showError('Update Error: $e');
    } finally {
      setState(() {
        _isUpdating = false;
      });
    }
  }

  void _removeItem(CartDetailModel detail) async {
    if (_isUpdating) return;
    setState(() {
      _isUpdating = true;
    });

    try {
      final success = await _apiService.removeFromCart(detail.cartDetailId);
      if (!mounted) return;
      if (success) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Item removed from cart.'), backgroundColor: Colors.teal),
        );
        _loadCart();
      } else {
        _showError('Failed to remove item.');
      }
    } catch (e) {
      _showError('Remove Error: $e');
    } finally {
      setState(() {
        _isUpdating = false;
      });
    }
  }

  void _clearAllCart() {
    if (_currentCart == null || _currentCart!.cartDetails.isEmpty) return;

    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Clear Cart'),
        content: const Text('Are you sure you want to remove all items from your cart?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('Cancel'),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
            onPressed: () async {
              Navigator.pop(ctx);
              setState(() {
                _isUpdating = true;
              });
              try {
                final ok = await _apiService.clearCart(_apiService.customerId!);
                if (ok) {
                  _loadCart();
                } else {
                  _showError('Failed to clear cart.');
                }
              } catch (e) {
                _showError('Error clearing cart: $e');
              } finally {
                setState(() {
                  _isUpdating = false;
                });
              }
            },
            child: const Text('Clear All', style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );
  }

  void _showError(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message), backgroundColor: AppColors.error),
    );
  }

  void _startCheckout() {
    final selected = _selectedItems;

    if (selected.isEmpty) {
      _showError('Please select at least 1 item to proceed to checkout.');
      return;
    }

    if (selected.length > 10) {
      _showError('Each order can contain a maximum of 10 items.');
      return;
    }

    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => CheckoutScreen(selectedItems: selected),
      ),
    ).then((_) => _loadCart());
  }

  @override
  Widget build(BuildContext context) {
    if (_apiService.token == null) {
      return Scaffold(
        appBar: AppBar(title: const Text('Shopping Cart'), backgroundColor: AppColors.primary, foregroundColor: Colors.white),
        body: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Icon(Icons.shopping_cart_outlined, size: 80, color: Colors.grey),
              const SizedBox(height: 16),
              const Text('Please login to view your shopping cart.'),
              const SizedBox(height: 16),
              ElevatedButton(
                onPressed: () => Navigator.pushNamed(context, '/login'),
                child: const Text('Login Now'),
              ),
            ],
          ),
        ),
      );
    }

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('My Shopping Cart'),
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
        actions: [
          if (_currentCart != null && _currentCart!.cartDetails.isNotEmpty)
            IconButton(
              icon: const Icon(Icons.delete_sweep_outlined),
              tooltip: 'Clear Cart',
              onPressed: _clearAllCart,
            ),
        ],
      ),
      body: FutureBuilder<CartResponseModel>(
        future: _cartFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting && _currentCart == null) {
            return const Center(child: CircularProgressIndicator());
          }

          if (snapshot.hasError) {
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Icon(Icons.error_outline, size: 64, color: AppColors.error),
                  const SizedBox(height: 16),
                  Text('API Connection Error: ${snapshot.error}', style: const TextStyle(color: AppColors.error), textAlign: TextAlign.center),
                  const SizedBox(height: 16),
                  ElevatedButton(onPressed: _loadCart, child: const Text('Reload')),
                ],
              ),
            );
          }

          if (_currentCart == null || _currentCart!.cartDetails.isEmpty) {
            return const Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(Icons.shopping_cart_outlined, size: 80, color: Colors.grey),
                  SizedBox(height: 16),
                  Text('Your cart is empty.', style: TextStyle(fontSize: 16)),
                ],
              ),
            );
          }

          return Column(
            children: [
              // Header Select All Bar
              Container(
                color: Colors.white,
                padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                child: Row(
                  children: [
                    Checkbox(
                      value: _isAllSelected,
                      activeColor: AppColors.primary,
                      onChanged: _toggleSelectAll,
                    ),
                    const Text(
                      'Select All Items',
                      style: TextStyle(fontWeight: FontWeight.bold),
                    ),
                    const Spacer(),
                    Text(
                      'Selected (${_selectedItems.length}/${_currentCart!.cartDetails.length})',
                      style: const TextStyle(color: AppColors.textSecondary, fontSize: 13),
                    ),
                  ],
                ),
              ),

              // Items List
              Expanded(
                child: ListView.builder(
                  padding: const EdgeInsets.all(16),
                  itemCount: _currentCart!.cartDetails.length,
                  itemBuilder: (context, index) {
                    final detail = _currentCart!.cartDetails[index];
                    final product = detail.product;

                    if (product == null) {
                      return Card(
                        child: ListTile(
                          title: const Text('Loading product details...'),
                          subtitle: Text('ID: ${detail.productId}'),
                          trailing: IconButton(
                            icon: const Icon(Icons.delete_outline, color: Colors.red),
                            onPressed: () => _removeItem(detail),
                          ),
                        ),
                      );
                    }

                    return Card(
                      margin: const EdgeInsets.only(bottom: 16),
                      elevation: 2,
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                      child: Padding(
                        padding: const EdgeInsets.all(8),
                        child: Row(
                          children: [
                            Checkbox(
                              value: detail.isSelected,
                              activeColor: AppColors.primary,
                              onChanged: (val) {
                                setState(() {
                                  detail.isSelected = val ?? false;
                                });
                              },
                            ),
                            ClipRRect(
                              borderRadius: BorderRadius.circular(8),
                              child: Container(
                                width: 72,
                                height: 72,
                                color: Colors.grey.shade100,
                                child: product.images.isNotEmpty
                                    ? Image.network(
                                        product.images.first,
                                        fit: BoxFit.cover,
                                        errorBuilder: (context, error, stackTrace) =>
                                            const Icon(Icons.shopping_bag, color: Colors.grey),
                                      )
                                    : const Icon(Icons.shopping_bag, color: Colors.grey),
                              ),
                            ),
                            const SizedBox(width: 12),
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    product.productName,
                                    maxLines: 1,
                                    overflow: TextOverflow.ellipsis,
                                    style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15),
                                  ),
                                  const SizedBox(height: 4),
                                  Text(
                                    '${product.productPrice.toStringAsFixed(0)}đ',
                                    style: const TextStyle(color: AppColors.primary, fontWeight: FontWeight.bold),
                                  ),
                                  const SizedBox(height: 8),
                                  Row(
                                    children: [
                                      IconButton(
                                        padding: EdgeInsets.zero,
                                        constraints: const BoxConstraints(),
                                        icon: const Icon(Icons.remove_circle_outline, size: 22),
                                        onPressed: () => _updateQuantity(detail, detail.quantity - 1),
                                      ),
                                      Padding(
                                        padding: const EdgeInsets.symmetric(horizontal: 12),
                                        child: Text(
                                          '${detail.quantity}',
                                          style: const TextStyle(fontSize: 15, fontWeight: FontWeight.bold),
                                        ),
                                      ),
                                      IconButton(
                                        padding: EdgeInsets.zero,
                                        constraints: const BoxConstraints(),
                                        icon: const Icon(Icons.add_circle_outline, size: 22),
                                        onPressed: () => _updateQuantity(detail, detail.quantity + 1),
                                      ),
                                    ],
                                  ),
                                ],
                              ),
                            ),
                            IconButton(
                              icon: const Icon(Icons.delete_outline, color: AppColors.error),
                              onPressed: () => _removeItem(detail),
                            ),
                          ],
                        ),
                      ),
                    );
                  },
                ),
              ),

              // Bottom Total & Checkout Bar
              Card(
                margin: EdgeInsets.zero,
                shape: const RoundedRectangleBorder(
                  borderRadius: BorderRadius.only(topLeft: Radius.circular(20), topRight: Radius.circular(20)),
                ),
                elevation: 8,
                child: Padding(
                  padding: const EdgeInsets.all(20),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          const Text('Total Payment:', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
                          Text(
                            '${_calculateTotal().toStringAsFixed(0)}đ',
                            style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: AppColors.primary),
                          ),
                        ],
                      ),
                      const SizedBox(height: 16),
                      CustomButton(
                        text: 'Proceed to Checkout (${_selectedItems.length})',
                        isLoading: _isUpdating,
                        onPressed: _startCheckout,
                      ),
                    ],
                  ),
                ),
              ),
            ],
          );
        },
      ),
    );
  }
}
