import 'package:flutter/material.dart';
import '../../../constants/app_colors.dart';
import '../../../models/cart_model.dart';
import '../../../models/address_model.dart';
import '../../../services/api_service.dart';
import '../../../widgets/custom_button.dart';

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

  double _calculateTotal() {
    if (_currentCart == null) return 0.0;
    double total = 0.0;
    for (var detail in _currentCart!.cartDetails) {
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

  void _showError(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message), backgroundColor: AppColors.error),
    );
  }

  // CHECKOUT FLOW
  void _startCheckout() async {
    if (_currentCart == null || _currentCart!.cartDetails.isEmpty) return;

    setState(() {
      _isUpdating = true;
    });

    try {
      final List<AddressModel> addresses = await _apiService.getMyAddresses();
      
      if (!mounted) return;
      setState(() {
        _isUpdating = false;
      });

      if (addresses.isEmpty) {
        _showAddAddressDialog();
      } else {
        _showSelectAddressDialog(addresses);
      }
    } catch (e) {
      setState(() {
        _isUpdating = false;
      });
      _showError('Failed to load shipping addresses: $e');
    }
  }

  // Dialog to add new address
  void _showAddAddressDialog() {
    final formKey = GlobalKey<FormState>();
    final provinceCtrl = TextEditingController();
    final districtCtrl = TextEditingController();
    final wardCtrl = TextEditingController();
    final detailsCtrl = TextEditingController();
    bool isDefault = true;

    showDialog(
      context: context,
      builder: (context) {
        return AlertDialog(
          title: const Text('Add Shipping Address'),
          content: SingleChildScrollView(
            child: Form(
              key: formKey,
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  TextFormField(
                    controller: provinceCtrl,
                    decoration: const InputDecoration(labelText: 'Province / City'),
                    validator: (v) => v == null || v.isEmpty ? 'Cannot be empty' : null,
                  ),
                  TextFormField(
                    controller: districtCtrl,
                    decoration: const InputDecoration(labelText: 'District'),
                    validator: (v) => v == null || v.isEmpty ? 'Cannot be empty' : null,
                  ),
                  TextFormField(
                    controller: wardCtrl,
                    decoration: const InputDecoration(labelText: 'Ward'),
                    validator: (v) => v == null || v.isEmpty ? 'Cannot be empty' : null,
                  ),
                  TextFormField(
                    controller: detailsCtrl,
                    decoration: const InputDecoration(labelText: 'Street Name, House No.'),
                    validator: (v) => v == null || v.isEmpty ? 'Cannot be empty' : null,
                  ),
                ],
              ),
            ),
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(context),
              child: const Text('Cancel'),
            ),
            ElevatedButton(
              onPressed: () async {
                if (formKey.currentState!.validate()) {
                  Navigator.pop(context);
                  setState(() {
                    _isUpdating = true;
                  });
                  try {
                    final ok = await _apiService.addAddress(
                      province: provinceCtrl.text.trim(),
                      district: districtCtrl.text.trim(),
                      ward: wardCtrl.text.trim(),
                      addressDetails: detailsCtrl.text.trim(),
                      isDefault: isDefault,
                    );
                    if (ok) {
                      _startCheckout();
                    } else {
                      _showError('Failed to add address.');
                    }
                  } catch (e) {
                    _showError('Address Error: $e');
                  } finally {
                    setState(() {
                      _isUpdating = false;
                    });
                  }
                }
              },
              child: const Text('Save & Continue'),
            ),
          ],
        );
      },
    );
  }

  // Dialog to select shipping address & confirm order
  void _showSelectAddressDialog(List<AddressModel> addresses) {
    AddressModel selectedAddress = addresses.firstWhere((a) => a.isDefault, orElse: () => addresses.first);

    showDialog(
      context: context,
      builder: (context) {
        return StatefulBuilder(
          builder: (context, setDialogState) {
            return AlertDialog(
              title: const Text('Select Shipping Address'),
              content: SizedBox(
                width: double.maxFinite,
                child: ListView.builder(
                  shrinkWrap: true,
                  itemCount: addresses.length,
                  itemBuilder: (context, index) {
                    final addr = addresses[index];
                    return RadioListTile<AddressModel>(
                      title: Text(addr.fullAddress, style: const TextStyle(fontSize: 14)),
                      subtitle: addr.isDefault ? const Text('Default', style: TextStyle(color: Colors.teal, fontSize: 11)) : null,
                      value: addr,
                      groupValue: selectedAddress,
                      onChanged: (val) {
                        setDialogState(() {
                          selectedAddress = val!;
                        });
                      },
                    );
                  },
                ),
              ),
              actions: [
                TextButton(
                  onPressed: () => _showAddAddressDialog(),
                  child: const Text('New Address'),
                ),
                ElevatedButton(
                  style: ElevatedButton.styleFrom(backgroundColor: AppColors.primary, foregroundColor: Colors.white),
                  onPressed: () async {
                    Navigator.pop(context);
                    _confirmOrder(selectedAddress);
                  },
                  child: const Text('Place COD Order'),
                ),
              ],
            );
          },
        );
      },
    );
  }

  // Submit order request
  void _confirmOrder(AddressModel address) async {
    setState(() {
      _isUpdating = true;
    });

    try {
      final result = await _apiService.placeCodOrder(
        addressId: address.addressId,
        items: _currentCart!.cartDetails,
      );

      if (mounted) {
        final isSuccess = result['success'] == true || result['Success'] == true || result['orderId'] != null || result['OrderId'] != null;
        if (isSuccess) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('🎉 Order placed successfully! It is now being processed.'),
              backgroundColor: Colors.green,
            ),
          );
          _loadCart();
        } else {
          _showError(result['message'] ?? result['Message'] ?? 'Order placement failed.');
        }
      }
    } catch (e) {
      if (mounted) {
        _showError('Order Error: $e');
      }
    } finally {
      if (mounted) {
        setState(() {
          _isUpdating = false;
        });
      }
    }
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
                        padding: const EdgeInsets.all(12),
                        child: Row(
                          children: [
                            ClipRRect(
                              borderRadius: BorderRadius.circular(8),
                              child: Container(
                                width: 80,
                                height: 80,
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
                            const SizedBox(width: 16),
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    product.productName,
                                    maxLines: 1,
                                    overflow: TextOverflow.ellipsis,
                                    style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
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
                                        icon: const Icon(Icons.remove_circle_outline, size: 20),
                                        onPressed: () => _updateQuantity(detail, detail.quantity - 1),
                                      ),
                                      Text(
                                        '${detail.quantity}',
                                        style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                                      ),
                                      IconButton(
                                        icon: const Icon(Icons.add_circle_outline, size: 20),
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

              Card(
                margin: EdgeInsets.zero,
                shape: const RoundedRectangleBorder(
                  borderRadius: BorderRadius.only(topLeft: Radius.circular(20), topRight: Radius.circular(20)),
                ),
                elevation: 8,
                child: Padding(
                  padding: const EdgeInsets.all(24),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          const Text('Total:', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                          Text(
                            '${_calculateTotal().toStringAsFixed(0)}đ',
                            style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: AppColors.primary),
                          ),
                        ],
                      ),
                      const SizedBox(height: 16),
                      CustomButton(
                        text: 'Place COD Order',
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
