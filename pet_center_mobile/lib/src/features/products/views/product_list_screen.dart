import 'package:flutter/material.dart';
import '../../../constants/app_colors.dart';
import '../../../models/product_model.dart';
import '../../../services/api_service.dart';
import 'product_detail_screen.dart';

class ProductListScreen extends StatefulWidget {
  const ProductListScreen({super.key});

  @override
  State<ProductListScreen> createState() => _ProductListScreenState();
}

class _ProductListScreenState extends State<ProductListScreen> {
  final ApiService _apiService = ApiService();
  final ScrollController _scrollController = ScrollController();
  final TextEditingController _searchController = TextEditingController();

  List<ProductModel> _allProducts = [];
  List<ProductModel> _filteredProducts = [];

  bool _isInitialLoading = true;
  bool _isLoadingMore = false;
  bool _hasMore = true;
  String? _errorMessage;

  static const int _pageSize = 30;

  @override
  void initState() {
    super.initState();
    _loadInitialProducts();
    _scrollController.addListener(_onScroll);
    _searchController.addListener(_filterProducts);
  }

  @override
  void dispose() {
    _scrollController.dispose();
    _searchController.dispose();
    super.dispose();
  }

  // Initial fetch: first 30 products
  Future<void> _loadInitialProducts() async {
    setState(() {
      _isInitialLoading = true;
      _errorMessage = null;
      _hasMore = true;
      _allProducts.clear();
      _filteredProducts.clear();
    });

    try {
      final list = await _apiService.getProducts(top: _pageSize, skip: 0);
      if (mounted) {
        setState(() {
          _allProducts = list;
          _filteredProducts = List.from(list);
          _hasMore = list.length >= _pageSize;
          _isInitialLoading = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _errorMessage = e.toString().replaceAll('Exception: ', '');
          _isInitialLoading = false;
        });
      }
    }
  }

  // Infinite Scroll: Load next batch of 30 products
  void _onScroll() {
    if (_scrollController.position.pixels >= _scrollController.position.maxScrollExtent - 300) {
      if (!_isLoadingMore && _hasMore && _searchController.text.isEmpty) {
        _loadMoreProducts();
      }
    }
  }

  Future<void> _loadMoreProducts() async {
    setState(() {
      _isLoadingMore = true;
    });

    try {
      final nextBatch = await _apiService.getProducts(
        top: _pageSize,
        skip: _allProducts.length,
      );

      if (mounted) {
        setState(() {
          if (nextBatch.isEmpty) {
            _hasMore = false;
          } else {
            _allProducts.addAll(nextBatch);
            _filteredProducts = List.from(_allProducts);
            _hasMore = nextBatch.length >= _pageSize;
          }
          _isLoadingMore = false;
        });
      }
    } catch (_) {
      if (mounted) {
        setState(() {
          _isLoadingMore = false;
        });
      }
    }
  }

  // Client-side search filter
  void _filterProducts() {
    final query = _searchController.text.trim().toLowerCase();
    setState(() {
      if (query.isEmpty) {
        _filteredProducts = List.from(_allProducts);
      } else {
        _filteredProducts = _allProducts.where((p) {
          final nameMatch = p.productName.toLowerCase().contains(query);
          final brandMatch = p.brandName != null && p.brandName!.toLowerCase().contains(query);
          final catMatch = p.categoryName != null && p.categoryName!.toLowerCase().contains(query);
          return nameMatch || brandMatch || catMatch;
        }).toList();
      }
    });
  }

  // Add product to cart
  void _addProductToCart(ProductModel product, int quantity) async {
    if (_apiService.token == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Please login to add products to your cart.'),
          backgroundColor: Colors.orange,
        ),
      );
      Navigator.pushNamed(context, '/login');
      return;
    }

    try {
      final success = await _apiService.addToCart(product.productId, quantity);
      if (mounted) {
        if (success) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text('Added ${product.productName} to cart!'),
              backgroundColor: Colors.green,
            ),
          );
        } else {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('Failed to add item to cart. Please try again.'),
              backgroundColor: AppColors.error,
            ),
          );
        }
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Cart Error: $e'),
            backgroundColor: AppColors.error,
          ),
        );
      }
    }
  }

  void _showProductDetails(ProductModel product) {
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => ProductDetailScreen(product: product),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Pet Store'),
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
        elevation: 0,
      ),
      body: Column(
        children: [
          // Search bar
          Padding(
            padding: const EdgeInsets.all(16.0),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Search products, brands, categories...',
                prefixIcon: const Icon(Icons.search),
                suffixIcon: _searchController.text.isNotEmpty
                    ? IconButton(
                        icon: const Icon(Icons.clear, size: 20),
                        onPressed: () {
                          _searchController.clear();
                        },
                      )
                    : null,
                border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                filled: true,
                fillColor: Colors.white,
              ),
            ),
          ),

          // Main product grid with RefreshIndicator & Pagination
          Expanded(
            child: RefreshIndicator(
              onRefresh: _loadInitialProducts,
              color: AppColors.primary,
              child: _buildBodyContent(),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildBodyContent() {
    if (_isInitialLoading) {
      return GridView.builder(
        padding: const EdgeInsets.symmetric(horizontal: 16),
        gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
          crossAxisCount: 2,
          childAspectRatio: 0.72,
          crossAxisSpacing: 16,
          mainAxisSpacing: 16,
        ),
        itemCount: 6, // 6 skeleton placeholders
        itemBuilder: (context, index) => _buildSkeletonCard(),
      );
    }

    if (_errorMessage != null) {
      return ListView(
        children: [
          SizedBox(
            height: MediaQuery.of(context).size.height * 0.5,
            child: Center(
              child: Padding(
                padding: const EdgeInsets.all(24.0),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    const Icon(Icons.error_outline, size: 64, color: AppColors.error),
                    const SizedBox(height: 16),
                    Text(
                      _errorMessage!,
                      style: const TextStyle(color: AppColors.error, fontSize: 14),
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 16),
                    ElevatedButton.icon(
                      style: ElevatedButton.styleFrom(
                        backgroundColor: AppColors.primary,
                        foregroundColor: Colors.white,
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                      ),
                      onPressed: _loadInitialProducts,
                      icon: const Icon(Icons.refresh),
                      label: const Text('Try Again'),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ],
      );
    }

    if (_filteredProducts.isEmpty) {
      return ListView(
        children: [
          SizedBox(
            height: MediaQuery.of(context).size.height * 0.5,
            child: const Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text('🐾', style: TextStyle(fontSize: 48)),
                  SizedBox(height: 12),
                  Text(
                    'No products found.',
                    style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: AppColors.textSecondary),
                  ),
                ],
              ),
            ),
          ),
        ],
      );
    }

    return CustomScrollView(
      controller: _scrollController,
      physics: const AlwaysScrollableScrollPhysics(),
      slivers: [
        SliverPadding(
          padding: const EdgeInsets.symmetric(horizontal: 16),
          sliver: SliverGrid(
            gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
              crossAxisCount: 2,
              childAspectRatio: 0.72,
              crossAxisSpacing: 16,
              mainAxisSpacing: 16,
            ),
            delegate: SliverChildBuilderDelegate(
              (context, index) {
                final product = _filteredProducts[index];
                return _buildProductCard(product);
              },
              childCount: _filteredProducts.length,
            ),
          ),
        ),

        // Infinite scroll loading spinner at bottom
        if (_isLoadingMore)
          const SliverToBoxAdapter(
            child: Padding(
              padding: EdgeInsets.symmetric(vertical: 20),
              child: Center(
                child: CircularProgressIndicator(color: AppColors.primary),
              ),
            ),
          ),

        const SliverToBoxAdapter(
          child: SizedBox(height: 24),
        ),
      ],
    );
  }

  Widget _buildProductCard(ProductModel product) {
    return GestureDetector(
      onTap: () => _showProductDetails(product),
      child: Card(
        elevation: 2,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Expanded(
              child: Container(
                decoration: BoxDecoration(
                  color: Colors.grey.shade100,
                  borderRadius: const BorderRadius.only(
                    topLeft: Radius.circular(12),
                    topRight: Radius.circular(12),
                  ),
                ),
                child: ClipRRect(
                  borderRadius: const BorderRadius.only(
                    topLeft: Radius.circular(12),
                    topRight: Radius.circular(12),
                  ),
                  child: product.images.isNotEmpty
                      ? Image.network(
                          product.images.first,
                          fit: BoxFit.cover,
                          errorBuilder: (context, error, stackTrace) =>
                              const Icon(Icons.shopping_bag, size: 50, color: Colors.grey),
                        )
                      : const Icon(Icons.shopping_bag, size: 50, color: Colors.grey),
                ),
              ),
            ),
            Padding(
              padding: const EdgeInsets.all(12.0),
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
                    product.brandName ?? 'Unknown Brand',
                    style: const TextStyle(fontSize: 11, color: AppColors.textSecondary),
                  ),
                  const SizedBox(height: 8),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Text(
                        '${product.productPrice.toStringAsFixed(0)}đ',
                        style: const TextStyle(
                          fontWeight: FontWeight.bold,
                          color: AppColors.primary,
                          fontSize: 15,
                        ),
                      ),
                      IconButton(
                        icon: const Icon(Icons.add_shopping_cart, color: AppColors.primary),
                        onPressed: () => _addProductToCart(product, 1),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildSkeletonCard() {
    return Card(
      elevation: 1,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Expanded(
            child: Container(
              decoration: BoxDecoration(
                color: Colors.grey.shade200,
                borderRadius: const BorderRadius.only(
                  topLeft: Radius.circular(12),
                  topRight: Radius.circular(12),
                ),
              ),
            ),
          ),
          Padding(
            padding: const EdgeInsets.all(12.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Container(height: 14, width: 100, color: Colors.grey.shade300),
                const SizedBox(height: 6),
                Container(height: 10, width: 60, color: Colors.grey.shade200),
                const SizedBox(height: 12),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Container(height: 16, width: 70, color: Colors.grey.shade300),
                    Container(width: 24, height: 24, decoration: BoxDecoration(color: Colors.grey.shade300, shape: BoxShape.circle)),
                  ],
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
