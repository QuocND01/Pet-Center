import 'package:flutter/material.dart';
import '../../../constants/app_colors.dart';
import '../../../models/product_model.dart';
import '../../../models/product_feedback_model.dart';
import '../../../services/api_service.dart';
import '../../../utils/app_error_utils.dart';
import '../widgets/product_feedback_widgets.dart';

class ProductDetailScreen extends StatefulWidget {
  final ProductModel? product;
  final String? productId;

  const ProductDetailScreen({
    super.key,
    this.product,
    this.productId,
  }) : assert(product != null || productId != null, 'Either product or productId must be provided.');

  @override
  State<ProductDetailScreen> createState() => _ProductDetailScreenState();
}

class _ProductDetailScreenState extends State<ProductDetailScreen> {
  final ApiService _apiService = ApiService();
  
  ProductModel? _product;
  late String _targetProductId;
  bool _isLoadingProduct = false;
  String? _productError;

  int _quantityToBuy = 1;
  int _selectedImageIndex = 0;
  bool _isAddingToCart = false;

  // Feedback State
  bool _isLoadingFeedbacks = true;
  String? _feedbackError;
  List<ProductFeedbackModel> _allFeedbacks = [];
  int _selectedStarFilter = 0; // 0 = All, 1..5 = stars, 6 = With media
  int _visibleCount = 5;

  @override
  void initState() {
    super.initState();
    _product = widget.product;
    _targetProductId = widget.product?.productId ?? widget.productId!;
    _loadProductDetails();
    _loadFeedbacks();
  }

  Future<void> _loadProductDetails() async {
    if (_product == null) {
      setState(() {
        _isLoadingProduct = true;
        _productError = null;
      });
    }

    try {
      final freshProduct = await _apiService.getProductDetails(_targetProductId);
      if (mounted) {
        setState(() {
          _product = freshProduct;
          _isLoadingProduct = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _productError = AppErrorUtils.getFriendlyMessage(e);
          _isLoadingProduct = false;
        });
      }
    }
  }

  Future<void> _loadFeedbacks() async {
    setState(() {
      _isLoadingFeedbacks = true;
      _feedbackError = null;
    });

    try {
      final list = await _apiService.getFeedbacksByProductId(_targetProductId);
      if (mounted) {
        setState(() {
          _allFeedbacks = list;
          _isLoadingFeedbacks = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _feedbackError = AppErrorUtils.getFriendlyMessage(e);
          _isLoadingFeedbacks = false;
        });
      }
    }
  }

  // Calculate statistics
  double get _avgRating {
    if (_allFeedbacks.isEmpty) return 0.0;
    final total = _allFeedbacks.fold<int>(0, (sum, item) => sum + item.rating);
    return total / _allFeedbacks.length;
  }

  Map<int, int> get _ratingCounts {
    final counts = <int, int>{1: 0, 2: 0, 3: 0, 4: 0, 5: 0};
    for (var f in _allFeedbacks) {
      if (counts.containsKey(f.rating)) {
        counts[f.rating] = counts[f.rating]! + 1;
      }
    }
    return counts;
  }

  List<ProductFeedbackModel> get _filteredFeedbacks {
    if (_selectedStarFilter == 0) return _allFeedbacks;
    if (_selectedStarFilter >= 1 && _selectedStarFilter <= 5) {
      return _allFeedbacks.where((f) => f.rating == _selectedStarFilter).toList();
    }
    if (_selectedStarFilter == 6) {
      return _allFeedbacks.where((f) => f.mediaFiles.isNotEmpty).toList();
    }
    return _allFeedbacks;
  }

  void _addToCart() async {
    if (_product == null) return;

    if (_apiService.token == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Please sign in to add products to your cart.'),
          backgroundColor: Colors.orange,
        ),
      );
      Navigator.pushNamed(context, '/login');
      return;
    }

    setState(() {
      _isAddingToCart = true;
    });

    try {
      final success = await _apiService.addToCart(_product!.productId, _quantityToBuy);
      if (mounted) {
        if (success) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text('Added ${_product!.productName} to cart!'),
              backgroundColor: AppColors.success,
            ),
          );
        } else {
          AppErrorUtils.showErrorSnackBar(context, 'Failed to add item to cart. Please try again.');
        }
      }
    } catch (e) {
      if (mounted) {
        AppErrorUtils.showErrorSnackBar(context, e);
      }
    } finally {
      if (mounted) {
        setState(() {
          _isAddingToCart = false;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoadingProduct && _product == null) {
      return Scaffold(
        backgroundColor: AppColors.background,
        appBar: AppBar(
          title: const Text('Product Details'),
          backgroundColor: AppColors.primary,
          foregroundColor: Colors.white,
        ),
        body: const Center(
          child: CircularProgressIndicator(color: AppColors.primary),
        ),
      );
    }

    if (_productError != null && _product == null) {
      return Scaffold(
        backgroundColor: AppColors.background,
        appBar: AppBar(
          title: const Text('Product Details'),
          backgroundColor: AppColors.primary,
          foregroundColor: Colors.white,
        ),
        body: Center(
          child: Padding(
            padding: const EdgeInsets.all(24.0),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                const Icon(Icons.signal_wifi_off_rounded, size: 64, color: AppColors.textSecondary),
                const SizedBox(height: 16),
                Text(
                  _productError!,
                  textAlign: TextAlign.center,
                  style: const TextStyle(fontSize: 14, color: AppColors.textPrimary),
                ),
                const SizedBox(height: 20),
                ElevatedButton.icon(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppColors.primary,
                    foregroundColor: Colors.white,
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                  ),
                  onPressed: _loadProductDetails,
                  icon: const Icon(Icons.refresh_rounded, size: 18),
                  label: const Text('Retry', style: TextStyle(fontWeight: FontWeight.bold)),
                ),
              ],
            ),
          ),
        ),
      );
    }

    final product = _product!;
    final filtered = _filteredFeedbacks;
    final currentVisibleFeedbacks = filtered.take(_visibleCount).toList();
    final remainingCount = filtered.length - currentVisibleFeedbacks.length;

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Text(
          product.productName,
          style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
        ),
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
        elevation: 0,
      ),
      body: Column(
        children: [
          Expanded(
            child: SingleChildScrollView(
              padding: const EdgeInsets.only(bottom: 24),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // 1. PRODUCT IMAGES CAROUSEL
                  Container(
                    width: double.infinity,
                    height: 280,
                    color: Colors.white,
                    child: Stack(
                      children: [
                        PageView.builder(
                          itemCount: product.images.isNotEmpty ? product.images.length : 1,
                          onPageChanged: (index) {
                            setState(() {
                              _selectedImageIndex = index;
                            });
                          },
                          itemBuilder: (context, index) {
                            if (product.images.isEmpty) {
                              return const Center(
                                child: Icon(Icons.pets, size: 80, color: Colors.grey),
                              );
                            }
                            return Image.network(
                              product.images[index],
                              fit: BoxFit.contain,
                              errorBuilder: (context, error, stackTrace) =>
                                  const Icon(Icons.broken_image, size: 80, color: Colors.grey),
                            );
                          },
                        ),
                        if (product.images.length > 1)
                          Positioned(
                            bottom: 12,
                            left: 0,
                            right: 0,
                            child: Row(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: List.generate(
                                product.images.length,
                                (index) => AnimatedContainer(
                                  duration: const Duration(milliseconds: 200),
                                  margin: const EdgeInsets.symmetric(horizontal: 3),
                                  width: _selectedImageIndex == index ? 16 : 8,
                                  height: 8,
                                  decoration: BoxDecoration(
                                    color: _selectedImageIndex == index
                                        ? AppColors.primary
                                        : Colors.grey.shade400,
                                    borderRadius: BorderRadius.circular(4),
                                  ),
                                ),
                              ),
                            ),
                          ),
                      ],
                    ),
                  ),

                  // 2. PRODUCT INFO CARD
                  Container(
                    padding: const EdgeInsets.all(20),
                    color: Colors.white,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        // Category & Brand Badges
                        Wrap(
                          spacing: 8,
                          runSpacing: 6,
                          children: [
                            if (product.categoryName != null)
                              Container(
                                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                                decoration: BoxDecoration(
                                  color: AppColors.primary.withValues(alpha: 0.12),
                                  borderRadius: BorderRadius.circular(100),
                                ),
                                child: Text(
                                  product.categoryName!,
                                  style: const TextStyle(
                                    fontSize: 11,
                                    fontWeight: FontWeight.bold,
                                    color: AppColors.primary,
                                  ),
                                ),
                              ),
                            if (product.brandName != null)
                              Container(
                                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                                decoration: BoxDecoration(
                                  color: Colors.blue.shade50,
                                  borderRadius: BorderRadius.circular(100),
                                ),
                                child: Text(
                                  product.brandName!,
                                  style: TextStyle(
                                    fontSize: 11,
                                    fontWeight: FontWeight.bold,
                                    color: Colors.blue.shade800,
                                  ),
                                ),
                              ),
                            Container(
                              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                              decoration: BoxDecoration(
                                color: product.stockQuantity > 0 ? Colors.green.shade50 : Colors.red.shade50,
                                borderRadius: BorderRadius.circular(100),
                              ),
                              child: Text(
                                product.stockQuantity > 0
                                    ? '✓ In Stock (${product.stockQuantity})'
                                    : '✕ Out of Stock',
                                style: TextStyle(
                                  fontSize: 11,
                                  fontWeight: FontWeight.bold,
                                  color: product.stockQuantity > 0 ? Colors.green.shade800 : Colors.red.shade800,
                                ),
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 12),

                        // Title
                        Text(
                          product.productName,
                          style: const TextStyle(
                            fontSize: 22,
                            fontWeight: FontWeight.bold,
                            color: AppColors.textPrimary,
                            height: 1.2,
                          ),
                        ),
                        const SizedBox(height: 10),

                        // Price
                        Text(
                          '${product.productPrice.toStringAsFixed(0)}đ',
                          style: const TextStyle(
                            fontSize: 26,
                            fontWeight: FontWeight.bold,
                            color: AppColors.primary,
                          ),
                        ),
                        const SizedBox(height: 16),

                        // Description
                        if (product.productDescription != null &&
                            product.productDescription!.trim().isNotEmpty) ...[
                          const Text(
                            'Product Description',
                            style: TextStyle(
                              fontSize: 15,
                              fontWeight: FontWeight.bold,
                              color: AppColors.textPrimary,
                            ),
                          ),
                          const SizedBox(height: 6),
                          Text(
                            product.productDescription!,
                            style: const TextStyle(
                              fontSize: 14,
                              color: AppColors.textSecondary,
                              height: 1.5,
                            ),
                          ),
                        ],
                      ],
                    ),
                  ),

                  const SizedBox(height: 12),

                  // 3. REVIEWS & FEEDBACK SECTION
                  Container(
                    padding: const EdgeInsets.all(20),
                    color: Colors.white,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        // Section Header
                        Row(
                          children: [
                            const Text(
                              '⭐ Customer Reviews',
                              style: TextStyle(
                                fontSize: 18,
                                fontWeight: FontWeight.bold,
                                color: AppColors.textPrimary,
                              ),
                            ),
                            const SizedBox(width: 8),
                            if (_allFeedbacks.isNotEmpty)
                              Container(
                                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                                decoration: BoxDecoration(
                                  color: AppColors.primary.withValues(alpha: 0.1),
                                  borderRadius: BorderRadius.circular(100),
                                ),
                                child: Text(
                                  '${_allFeedbacks.length} reviews',
                                  style: const TextStyle(
                                    fontSize: 12,
                                    fontWeight: FontWeight.bold,
                                    color: AppColors.primary,
                                  ),
                                ),
                              ),
                          ],
                        ),
                        const SizedBox(height: 16),

                        // Loading & Error States
                        if (_isLoadingFeedbacks)
                          const Padding(
                            padding: EdgeInsets.symmetric(vertical: 32),
                            child: Center(
                              child: CircularProgressIndicator(color: AppColors.primary),
                            ),
                          )
                        else if (_feedbackError != null)
                          Padding(
                            padding: const EdgeInsets.symmetric(vertical: 24),
                            child: Center(
                              child: Column(
                                children: [
                                  const Icon(Icons.error_outline, color: AppColors.error, size: 40),
                                  const SizedBox(height: 8),
                                  Text(
                                    _feedbackError!,
                                    style: const TextStyle(color: AppColors.error, fontSize: 13),
                                    textAlign: TextAlign.center,
                                  ),
                                  const SizedBox(height: 8),
                                  TextButton(
                                    onPressed: _loadFeedbacks,
                                    child: const Text('Retry'),
                                  ),
                                ],
                              ),
                            ),
                          )
                        else if (_allFeedbacks.isEmpty)
                          // Empty State
                          Container(
                            width: double.infinity,
                            padding: const EdgeInsets.symmetric(vertical: 36, horizontal: 16),
                            decoration: BoxDecoration(
                              color: AppColors.primary.withValues(alpha: 0.03),
                              borderRadius: BorderRadius.circular(16),
                              border: Border.all(color: Colors.grey.shade300, style: BorderStyle.solid),
                            ),
                            child: const Column(
                              children: [
                                Text('🐾', style: TextStyle(fontSize: 48)),
                                SizedBox(height: 10),
                                Text(
                                  'No reviews yet',
                                  style: TextStyle(
                                    fontSize: 16,
                                    fontWeight: FontWeight.bold,
                                    color: AppColors.textPrimary,
                                  ),
                                ),
                                SizedBox(height: 4),
                                Text(
                                  'Be the first to review this product!',
                                  style: TextStyle(fontSize: 12, color: AppColors.textSecondary),
                                  textAlign: TextAlign.center,
                                ),
                              ],
                            ),
                          )
                        else ...[
                          // Rating Summary Box
                          RatingSummaryCard(
                            avgRating: _avgRating,
                            totalCount: _allFeedbacks.length,
                            ratingCounts: _ratingCounts,
                          ),
                          const SizedBox(height: 16),

                          // Filter Chips
                          SingleChildScrollView(
                            scrollDirection: Axis.horizontal,
                            child: Row(
                              children: [
                                _buildFilterChip(label: 'All (${_allFeedbacks.length})', filterValue: 0),
                                _buildFilterChip(label: '5 ★ (${_ratingCounts[5]})', filterValue: 5),
                                _buildFilterChip(label: '4 ★ (${_ratingCounts[4]})', filterValue: 4),
                                _buildFilterChip(label: '3 ★ (${_ratingCounts[3]})', filterValue: 3),
                                _buildFilterChip(label: '2 ★ (${_ratingCounts[2]})', filterValue: 2),
                                _buildFilterChip(label: '1 ★ (${_ratingCounts[1]})', filterValue: 1),
                                _buildFilterChip(
                                  label: 'With Photos/Videos (${_allFeedbacks.where((f) => f.mediaFiles.isNotEmpty).length})',
                                  filterValue: 6,
                                ),
                              ],
                            ),
                          ),
                          const SizedBox(height: 16),

                          // Review List
                          if (filtered.isEmpty)
                            const Padding(
                              padding: EdgeInsets.symmetric(vertical: 24),
                              child: Center(
                                child: Text(
                                  'No reviews match this filter.',
                                  style: TextStyle(color: AppColors.textSecondary),
                                ),
                              ),
                            )
                          else
                            ListView.builder(
                              shrinkWrap: true,
                              physics: const NeverScrollableScrollPhysics(),
                              itemCount: currentVisibleFeedbacks.length,
                              itemBuilder: (context, index) {
                                return FeedbackCardWidget(
                                  feedback: currentVisibleFeedbacks[index],
                                );
                              },
                            ),

                          // Load More Button
                          if (remainingCount > 0) ...[
                            const SizedBox(height: 12),
                            Center(
                              child: OutlinedButton(
                                style: OutlinedButton.styleFrom(
                                  foregroundColor: AppColors.primary,
                                  side: const BorderSide(color: AppColors.primary, width: 1.5),
                                  shape: RoundedRectangleBorder(
                                    borderRadius: BorderRadius.circular(100),
                                  ),
                                  padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
                                ),
                                onPressed: () {
                                  setState(() {
                                    _visibleCount += 5;
                                  });
                                },
                                child: Text(
                                  'View More Reviews ($remainingCount left)',
                                  style: const TextStyle(fontWeight: FontWeight.bold),
                                ),
                              ),
                            ),
                          ],
                        ],
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),

          // 4. STICKY BOTTOM CART BAR
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
            decoration: BoxDecoration(
              color: Colors.white,
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withValues(alpha: 0.08),
                  blurRadius: 10,
                  offset: const Offset(0, -2),
                ),
              ],
            ),
            child: SafeArea(
              child: Row(
                children: [
                  // Quantity Stepper
                  Container(
                    decoration: BoxDecoration(
                      border: Border.all(color: Colors.grey.shade300),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Row(
                      children: [
                        IconButton(
                          icon: const Icon(Icons.remove, size: 20),
                          onPressed: () {
                            if (_quantityToBuy > 1) {
                              setState(() {
                                _quantityToBuy--;
                              });
                            }
                          },
                        ),
                        Text(
                          '$_quantityToBuy',
                          style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                        ),
                        IconButton(
                          icon: const Icon(Icons.add, size: 20),
                          onPressed: () {
                            if (_quantityToBuy < product.stockQuantity) {
                              setState(() {
                                _quantityToBuy++;
                              });
                            }
                          },
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(width: 16),

                  // Add to Cart Button
                  Expanded(
                    child: SizedBox(
                      height: 48,
                      child: ElevatedButton.icon(
                        style: ElevatedButton.styleFrom(
                          backgroundColor: AppColors.primary,
                          foregroundColor: Colors.white,
                          elevation: 2,
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(12),
                          ),
                        ),
                        onPressed: (product.stockQuantity > 0 && !_isAddingToCart) ? _addToCart : null,
                        icon: _isAddingToCart
                            ? const SizedBox(
                                width: 20,
                                height: 20,
                                child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2),
                              )
                            : const Icon(Icons.shopping_cart_outlined),
                        label: Text(
                          product.stockQuantity > 0
                              ? (_isAddingToCart ? 'Adding...' : 'Add to Cart')
                              : 'Out of Stock',
                          style: const TextStyle(fontSize: 15, fontWeight: FontWeight.bold),
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildFilterChip({required String label, required int filterValue}) {
    final isSelected = _selectedStarFilter == filterValue;

    return Padding(
      padding: const EdgeInsets.only(right: 8.0),
      child: FilterChip(
        selected: isSelected,
        label: Text(
          label,
          style: TextStyle(
            color: isSelected ? Colors.white : AppColors.textPrimary,
            fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
            fontSize: 12,
          ),
        ),
        backgroundColor: Colors.grey.shade100,
        selectedColor: AppColors.primary,
        checkmarkColor: Colors.white,
        showCheckmark: false,
        onSelected: (selected) {
          setState(() {
            _selectedStarFilter = filterValue;
            _visibleCount = 5;
          });
        },
      ),
    );
  }
}
