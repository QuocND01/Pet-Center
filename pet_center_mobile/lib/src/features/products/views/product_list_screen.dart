import 'package:flutter/material.dart';
import 'package:pet_center_mobile/src/models/ProductResponse.dart';
import 'package:pet_center_mobile/src/models/brand_model.dart';
import 'package:pet_center_mobile/src/models/category_model.dart';
import 'package:pet_center_mobile/src/models/product_model.dart';
import 'package:pet_center_mobile/src/services/api_service.dart';
class ProductPage extends StatefulWidget {
  const ProductPage({super.key});

  @override
  State<ProductPage> createState() => _ProductPageState();
}

class _ProductPageState extends State<ProductPage> {
  final ApiService _apiService = ApiService();

  Future<void> _changePage(int page) async {
    if (page < 1 || page > totalPages || page == currentPage) {
      return;
    }

    setState(() {
      currentPage = page;
    });

    await _loadProducts();
  }

// ============================================================
// DATA
// ============================================================
  int totalProducts = 0;
  List<ProductModel> products = [];
  List<ProductModel> hotProducts = [];
  List<ProductModel> newProducts = [];

  List<BrandModel> brands = [];
  List<CategoryModel> categories = [];

// ============================================================
// STATE
// ============================================================

  bool isLoading = false;
  String? errorMessage;

// ============================================================
// FILTER
// ============================================================

  final TextEditingController _searchController =
  TextEditingController();

  String? selectedCategoryId;
  String? selectedBrandId;

  double? minPrice;
  double? maxPrice;

  String? selectedSortBy;
  String selectedSortOrder = 'asc';

  int currentPage = 1;

  static const int pageSize = 24;

  int get totalPages =>
      (totalProducts / pageSize).ceil();

// ============================================================
// INIT
// ============================================================

  @override
  void initState() {
    super.initState();
    _loadData();
  }

// ============================================================
// LOAD ALL DATA
// ============================================================
  Future<void> _loadData() async {
    setState(() {
      isLoading = true;
      errorMessage = null;
    });

    try {
      final results = await Future.wait([
        _apiService.getProducts(
          search:
          _searchController.text.trim().isEmpty
              ? null
              : _searchController.text.trim(),
          categoryId:
          selectedCategoryId,
          brandId:
          selectedBrandId,
          minPrice:
          minPrice,
          maxPrice:
          maxPrice,
          sortBy:
          selectedSortBy,
          sortOrder:
          selectedSortOrder,
          page:
          currentPage,
        ),

        _apiService.getHotProducts(),

        _apiService.getNewProducts(),

        _apiService.getBrands(),

        _apiService.getCategories(),
      ]);

      if (!mounted) return;

      final productResponse =
      results[0] as ProductResponse;
      setState(() {
        products =
            productResponse.products;
        totalProducts = productResponse.count;

        hotProducts =
        results[1] as List<ProductModel>;

        newProducts =
        results[2] as List<ProductModel>;

        brands =
        results[3] as List<BrandModel>;

        categories =
        results[4] as List<CategoryModel>;

        isLoading = false;
      });
    } catch (e) {
      if (!mounted) return;

      setState(() {
        isLoading = false;
        errorMessage = e.toString();
      });
    }
  }

// ============================================================
// LOAD ONLY PRODUCTS
// Used when applying filters
// ============================================================
  Future<void> _loadProducts() async {
    setState(() {
      isLoading = true;
      errorMessage = null;
    });

    try {
      final result = await _apiService.getProducts(
        search: _searchController.text.trim().isEmpty
            ? null
            : _searchController.text.trim(),
        categoryId: selectedCategoryId,
        brandId: selectedBrandId,
        minPrice: minPrice,
        maxPrice: maxPrice,
        sortBy: selectedSortBy,
        sortOrder: selectedSortOrder,
        page: currentPage,
      );
      if (!mounted) return;

      setState(() {
        products = result.products;
        totalProducts = result.count;
        isLoading = false;
      });
    } catch (e) {
      if (!mounted) return;

      setState(() {
        isLoading = false;
        errorMessage = e.toString();
      });
    }
  }
// ============================================================
// APPLY FILTER
// ============================================================

  Future<void> _applyFilter() async {
    if (!mounted) return;

    setState(() {
      currentPage = 1;
    });

    await _loadProducts();
  }
// ============================================================
// RESET FILTER
// ============================================================

  void _resetFilter() {
    _searchController.clear();

    setState(() {
      selectedCategoryId = null;
      selectedBrandId = null;

      minPrice = null;
      maxPrice = null;

      selectedSortBy = null;
      selectedSortOrder = 'asc';

      currentPage = 1;
    });

    _loadProducts();
  }

// ============================================================
// BUILD
// ============================================================

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor:
      const Color(0xFFF7FDF9),

// ========================================================
// APP BAR
// ========================================================

      appBar: AppBar(
        title: const Text(
          'PetCenter Shop',
          style: TextStyle(
            fontWeight: FontWeight.w800,
          ),
        ),

        backgroundColor:
        Colors.white,

        foregroundColor:
        const Color(0xFF0F1F0F),

        elevation: 0,
      ),

// ========================================================
// BODY
// ========================================================

      body: RefreshIndicator(
        onRefresh: _loadData,

        child: CustomScrollView(
          physics:
          const AlwaysScrollableScrollPhysics(),

          slivers: [

// ====================================================
// HERO BANNER
// ====================================================

            SliverToBoxAdapter(
              child:
              _buildHeroBanner(),
            ),

// ====================================================
// SHOP INTRO
// ====================================================

            SliverToBoxAdapter(
              child:
              _buildShopIntro(),
            ),

// ====================================================
// HOT PRODUCTS
// ====================================================

            if (hotProducts.isNotEmpty)
              SliverToBoxAdapter(
                child:
                _buildProductSection(
                  title:
                  '🔥 Hot Products',

                  products:
                  hotProducts,
                ),
              ),

// ====================================================
// NEW PRODUCTS
// ====================================================

            if (newProducts.isNotEmpty)
              SliverToBoxAdapter(
                child:
                _buildProductSection(
                  title:
                  '🆕 New Products',

                  products:
                  newProducts,
                ),
              ),

// ====================================================
// FILTER
// ====================================================

            SliverToBoxAdapter(
              child:
              _buildFilterCard(),
            ),

// ====================================================
// RESULT COUNT
// ====================================================

            SliverToBoxAdapter(
              child:
              Padding(
                padding:
                const EdgeInsets.fromLTRB(
                  16,
                  20,
                  16,
                  10,
                ),

                child:
                Text(
                  '$totalProducts products found',
                  style:
                  const TextStyle(
                    fontSize: 15,

                    fontWeight:
                    FontWeight.w600,

                    color:
                    Color(0xFF6B7280),
                  ),
                ),
              ),
            ),

// ====================================================
// LOADING
// ====================================================

            if (isLoading)
              const SliverFillRemaining(
                child:
                Center(
                  child:
                  CircularProgressIndicator(
                    color:
                    Color(0xFF2ECC71),
                  ),
                ),
              )

// ====================================================
// ERROR
// ====================================================

            else if (errorMessage != null)
              SliverFillRemaining(
                child:
                _buildError(),
              )

// ====================================================
// EMPTY
// ====================================================

            else if (products.isEmpty)
                SliverFillRemaining(
                  child:
                  _buildEmptyState(),
                )

// ====================================================
// ALL PRODUCTS
// ====================================================
              else
                SliverMainAxisGroup(
                  slivers: [

                    // ====================================================
                    // PRODUCT GRID
                    // ====================================================

                    SliverPadding(
                      padding:
                      const EdgeInsets.fromLTRB(
                        16,
                        16,
                        16,
                        0,
                      ),

                      sliver:
                      SliverGrid.builder(
                        itemCount:
                        products.length,

                        gridDelegate:
                        const SliverGridDelegateWithFixedCrossAxisCount(
                          crossAxisCount:
                          2,

                          crossAxisSpacing:
                          12,

                          mainAxisSpacing:
                          12,

                          childAspectRatio:
                          0.65,
                        ),

                        itemBuilder:
                            (context, index) {
                          return _buildProductCard(
                            products[index],
                          );
                        },
                      ),
                    ),

                    // ====================================================
                    // PAGINATION
                    // ====================================================

                    SliverToBoxAdapter(
                      child:
                      _buildPagination(),
                    ),
                  ],
                ),
          ],
        ),
      ),
    );
  }

// ============================================================
// HERO BANNER
// ============================================================

  Widget _buildHeroBanner() {
    return Container(
      height: 230,

      margin:
      const EdgeInsets.all(16),

      clipBehavior:
      Clip.antiAlias,

      decoration:
      BoxDecoration(
        borderRadius:
        BorderRadius.circular(24),
      ),

      child:
      Stack(
        fit:
        StackFit.expand,

        children: [

// IMAGE
          Image.network(
            'https://res.cloudinary.com/dbjdxy4p6/image/upload/v1773461154/Water2_lsvuv0.png',

            fit:
            BoxFit.cover,

            errorBuilder:
                (_, __, ___) {
              return Container(
                color:
                const Color(0xFFDCFCE7),
              );
            },
          ),

// OVERLAY
          Container(
            padding:
            const EdgeInsets.all(24),

            decoration:
            const BoxDecoration(
              gradient:
              LinearGradient(
                colors: [
                  Colors.black54,
                  Colors.transparent,
                ],

                begin:
                Alignment.centerLeft,

                end:
                Alignment.centerRight,
              ),
            ),

            child:
            Column(
              mainAxisAlignment:
              MainAxisAlignment.center,

              crossAxisAlignment:
              CrossAxisAlignment.start,

              children: [

                const Text(
                  'Pet Center Shop',

                  style:
                  TextStyle(
                    color:
                    Colors.white,

                    fontSize:
                    28,

                    fontWeight:
                    FontWeight.w900,
                  ),
                ),

                const SizedBox(
                  height: 8,
                ),

                const Text(
                  'Best products for your lovely pets',

                  style:
                  TextStyle(
                    color:
                    Colors.white70,

                    fontSize:
                    14,
                  ),
                ),

                const SizedBox(
                  height: 16,
                ),

                ElevatedButton(
                  onPressed:
                  _loadProducts,

                  style:
                  ElevatedButton.styleFrom(
                    backgroundColor:
                    const Color(
                        0xFF2ECC71),

                    foregroundColor:
                    Colors.white,

                    shape:
                    RoundedRectangleBorder(
                      borderRadius:
                      BorderRadius.circular(
                          30),
                    ),
                  ),

                  child:
                  const Text(
                    'Shop Now →',
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

// ============================================================
// SHOP INTRO
// ============================================================

  Widget _buildShopIntro() {
    return Container(
      padding:
      const EdgeInsets.fromLTRB(
        20,
        20,
        20,
        30,
      ),

      child:
      Column(
        children: [

          Container(
            padding:
            const EdgeInsets.symmetric(
              horizontal:
              16,

              vertical:
              8,
            ),

            decoration:
            BoxDecoration(
              color:
              const Color(
                  0xFFECFDF5),

              borderRadius:
              BorderRadius.circular(
                  30),
            ),

            child:
            const Text(
              'PetCenter Shop',

              style:
              TextStyle(
                color:
                Color(0xFF2ECC71),

                fontWeight:
                FontWeight.w700,
              ),
            ),
          ),

          const SizedBox(
            height: 16,
          ),

          const Text(
            'Everything Your Pet Needs,\nAll in One Place',

            textAlign:
            TextAlign.center,

            style:
            TextStyle(
              fontSize:
              28,

              fontWeight:
              FontWeight.w900,

              color:
              Color(0xFF222222),
            ),
          ),

          const SizedBox(
            height: 16,
          ),

          const Text(
            'Browse our carefully selected collection of premium pet food, toys, healthcare essentials, grooming supplies and accessories.',

            textAlign:
            TextAlign.center,

            style:
            TextStyle(
              color:
              Color(0xFF6B7280),

              fontSize:
              14,

              height:
              1.6,
            ),
          ),

          const SizedBox(
            height: 20,
          ),

          Wrap(
            spacing:
            8,

            runSpacing:
            8,

            alignment:
            WrapAlignment.center,

            children:
            const [

              _FeatureChip(
                text:
                '✓ Premium Quality',
              ),

              _FeatureChip(
                text:
                '✓ Trusted Brands',
              ),

              _FeatureChip(
                text:
                '✓ Fast Delivery',
              ),

              _FeatureChip(
                text:
                '✓ Affordable Prices',
              ),
            ],
          ),
        ],
      ),
    );
  }

// ============================================================
// HOT / NEW PRODUCT SECTION
// ============================================================

  Widget _buildProductSection({
    required String title,
    required List<ProductModel> products,
  }) {
    return Column(
      crossAxisAlignment:
      CrossAxisAlignment.start,

      children: [

        Padding(
          padding:
          const EdgeInsets.fromLTRB(
            16,
            20,
            16,
            12,
          ),

          child:
          Text(
            title,

            style:
            const TextStyle(
              fontSize:
              22,

              fontWeight:
              FontWeight.w900,

              color:
              Color(0xFF222222),
            ),
          ),
        ),

        SizedBox(
          height:
          300,

          child:
          ListView.builder(
            scrollDirection:
            Axis.horizontal,

            padding:
            const EdgeInsets.symmetric(
              horizontal:
              16,
            ),

            itemCount:
            products.length,

            itemBuilder:
                (context, index) {

              return SizedBox(
                width:
                180,

                child:
                Padding(
                  padding:
                  const EdgeInsets.only(
                    right:
                    12,
                  ),

                  child:
                  _buildProductCard(
                    products[index],
                  ),
                ),
              );
            },
          ),
        ),
      ],
    );
  }

// ============================================================
// FILTER CARD
// ============================================================

  Widget _buildFilterCard() {
    return Container(
      margin:
      const EdgeInsets.all(16),

      padding:
      const EdgeInsets.all(16),

      decoration:
      BoxDecoration(
        color:
        Colors.white,

        borderRadius:
        BorderRadius.circular(
            20),

        border:
        Border.all(
          color:
          const Color(
              0xFFE5E7EB),
        ),
      ),

      child:
      Column(
        children: [

// ======================================================
// SEARCH
// ======================================================

          TextField(
            controller:
            _searchController,

            decoration:
            InputDecoration(
              hintText:
              'Search products...',

              prefixIcon:
              const Icon(
                Icons.search,
              ),

              filled:
              true,

              fillColor:
              const Color(
                  0xFFF7FDF9),

              border:
              OutlineInputBorder(
                borderRadius:
                BorderRadius.circular(
                    30),

                borderSide:
                BorderSide.none,
              ),
            ),

            onSubmitted:
                (_) => _applyFilter(),
          ),

          const SizedBox(
            height: 12,
          ),

// ======================================================
// CATEGORY + BRAND
// ======================================================

          Row(
            children: [

// CATEGORY
              Expanded(
                child:
                DropdownButtonFormField<
                    String?>(
                  value:
                  selectedCategoryId,

                  isExpanded:
                  true,

                  decoration:
                  InputDecoration(
                    labelText:
                    'Category',

                    border:
                    OutlineInputBorder(
                      borderRadius:
                      BorderRadius.circular(
                          15),
                    ),
                  ),

                  items: [

                    const DropdownMenuItem<
                        String?>(
                      value:
                      null,

                      child:
                      Text(
                        'All Categories',
                      ),
                    ),

                    ...categories.map(
                          (
                          category,
                          ) {
                        return DropdownMenuItem<
                            String?>(
                          value:
                          category.categoryId,

                          child:
                          Text(
                            category.categoryName,

                            overflow:
                            TextOverflow.ellipsis,
                          ),
                        );
                      },
                    ),
                  ],

                  onChanged:
                      (value) {
                    setState(() {
                      selectedCategoryId =
                          value;
                    });
                  },
                ),
              ),

              const SizedBox(
                width:
                10,
              ),

// BRAND
              Expanded(
                child:
                DropdownButtonFormField<
                    String?>(
                  value:
                  selectedBrandId,

                  isExpanded:
                  true,

                  decoration:
                  InputDecoration(
                    labelText:
                    'Brand',

                    border:
                    OutlineInputBorder(
                      borderRadius:
                      BorderRadius.circular(
                          15),
                    ),
                  ),

                  items: [

                    const DropdownMenuItem<
                        String?>(
                      value:
                      null,

                      child:
                      Text(
                        'All Brands',
                      ),
                    ),

                    ...brands.map(
                          (
                          brand,
                          ) {
                        return DropdownMenuItem<
                            String?>(
                          value:
                          brand.brandId,

                          child:
                          Text(
                            brand.brandName,

                            overflow:
                            TextOverflow.ellipsis,
                          ),
                        );
                      },
                    ),
                  ],

                  onChanged:
                      (value) {
                    setState(() {
                      selectedBrandId =
                          value;
                    });
                  },
                ),
              ),
            ],
          ),

          const SizedBox(
            height:
            12,
          ),

// ======================================================
// SORT
// ======================================================

          Row(
            children: [

              Expanded(
                child:
                DropdownButtonFormField<
                    String?>(
                  value:
                  selectedSortBy,

                  decoration:
                  InputDecoration(
                    labelText:
                    'Sort',

                    border:
                    OutlineInputBorder(
                      borderRadius:
                      BorderRadius.circular(
                          15),
                    ),
                  ),

                  items: const [

                    DropdownMenuItem<
                        String?>(
                      value:
                      null,

                      child:
                      Text(
                        'Default',
                      ),
                    ),

                    DropdownMenuItem<
                        String?>(
                      value:
                      'price',

                      child:
                      Text(
                        'Price',
                      ),
                    ),

                    DropdownMenuItem<
                        String?>(
                      value:
                      'name',

                      child:
                      Text(
                        'Name',
                      ),
                    ),

                    DropdownMenuItem<
                        String?>(
                      value:
                      'date',

                      child:
                      Text(
                        'Newest',
                      ),
                    ),
                  ],

                  onChanged:
                      (value) {
                    setState(() {
                      selectedSortBy =
                          value;
                    });
                  },
                ),
              ),

              const SizedBox(
                width:
                10,
              ),

              Expanded(
                child:
                DropdownButtonFormField<
                    String>(
                  value:
                  selectedSortOrder,

                  decoration:
                  InputDecoration(
                    labelText:
                    'Order',

                    border:
                    OutlineInputBorder(
                      borderRadius:
                      BorderRadius.circular(
                          15),
                    ),
                  ),

                  items: const [

                    DropdownMenuItem(
                      value:
                      'asc',

                      child:
                      Text(
                        'Ascending',
                      ),
                    ),

                    DropdownMenuItem(
                      value:
                      'desc',

                      child:
                      Text(
                        'Descending',
                      ),
                    ),
                  ],

                  onChanged:
                      (value) {

                    if (value ==
                        null) {
                      return;
                    }

                    setState(() {
                      selectedSortOrder =
                          value;
                    });
                  },
                ),
              ),
            ],
          ),

          const SizedBox(
            height:
            12,
          ),

// ======================================================
// PRICE
// ======================================================

          Row(
            children: [

              Expanded(
                child:
                TextField(
                  keyboardType:
                  TextInputType.number,

                  decoration:
                  InputDecoration(
                    hintText:
                    'Min Price',

                    border:
                    OutlineInputBorder(
                      borderRadius:
                      BorderRadius.circular(
                          15),
                    ),
                  ),

                  onChanged:
                      (value) {
                    minPrice =
                        double.tryParse(
                            value);
                  },
                ),
              ),

              const SizedBox(
                width:
                10,
              ),

              Expanded(
                child:
                TextField(
                  keyboardType:
                  TextInputType.number,

                  decoration:
                  InputDecoration(
                    hintText:
                    'Max Price',

                    border:
                    OutlineInputBorder(
                      borderRadius:
                      BorderRadius.circular(
                          15),
                    ),
                  ),

                  onChanged:
                      (value) {
                    maxPrice =
                        double.tryParse(
                            value);
                  },
                ),
              ),
            ],
          ),

          const SizedBox(
            height:
            16,
          ),

// ======================================================
// BUTTONS
// ======================================================

          Row(
            children: [

              Expanded(
                child:
                ElevatedButton.icon(
                  onPressed:
                  _applyFilter,

                  icon:
                  const Icon(
                    Icons.filter_alt,
                  ),

                  label:
                  const Text(
                    'Filter',
                  ),

                  style:
                  ElevatedButton.styleFrom(
                    backgroundColor:
                    const Color(
                        0xFF2ECC71),

                    foregroundColor:
                    Colors.white,

                    padding:
                    const EdgeInsets
                        .symmetric(
                      vertical:
                      14,
                    ),

                    shape:
                    RoundedRectangleBorder(
                      borderRadius:
                      BorderRadius.circular(
                          30),
                    ),
                  ),
                ),
              ),

              const SizedBox(
                width:
                10,
              ),

              Expanded(
                child:
                OutlinedButton(
                  onPressed:
                  _resetFilter,

                  style:
                  OutlinedButton.styleFrom(
                    padding:
                    const EdgeInsets
                        .symmetric(
                      vertical:
                      14,
                    ),

                    shape:
                    RoundedRectangleBorder(
                      borderRadius:
                      BorderRadius.circular(
                          30),
                    ),
                  ),

                  child:
                  const Text(
                    'Clear Filters',
                  ),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
// ============================================================
// PAGINATION
// ============================================================
  Widget _buildPagination() {
    if (totalPages <= 1) {
      return const SizedBox.shrink();
    }

    final startPage =
    (currentPage - 2).clamp(1, totalPages);

    final endPage =
    (startPage + 4).clamp(1, totalPages);

    return Padding(
      padding: const EdgeInsets.fromLTRB(
        16,
        24,
        16,
        32,
      ),
      child: Column(
        children: [
          // ==============================================
          // PAGE INFO
          // ==============================================

          Text(
            'Page $currentPage of $totalPages',
            style: const TextStyle(
              fontSize: 14,
              fontWeight: FontWeight.w600,
              color: Color(0xFF6B7280),
            ),
          ),

          const SizedBox(
            height: 12,
          ),

          // ==============================================
          // PAGINATION BUTTONS
          // ==============================================

          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              // PREVIOUS
              IconButton(
                onPressed: currentPage > 1
                    ? () => _changePage(
                  currentPage - 1,
                )
                    : null,
                icon: const Icon(
                  Icons.chevron_left,
                ),
              ),

              // PAGE NUMBERS
              ...List.generate(
                endPage - startPage + 1,
                    (index) {
                  final page = startPage + index;

                  return Padding(
                    padding: const EdgeInsets.symmetric(
                      horizontal: 3,
                    ),
                    child: SizedBox(
                      width: 40,
                      height: 40,
                      child: ElevatedButton(
                        onPressed: currentPage == page
                            ? null
                            : () => _changePage(page),
                        style: ElevatedButton.styleFrom(
                          padding: EdgeInsets.zero,
                          backgroundColor:
                          const Color(0xFF2ECC71),
                          disabledBackgroundColor:
                          const Color(0xFF166534),
                          foregroundColor:
                          Colors.white,
                          disabledForegroundColor:
                          Colors.white,
                          elevation: 0,
                          shape:
                          RoundedRectangleBorder(
                            borderRadius:
                            BorderRadius.circular(10),
                          ),
                        ),
                        child: Text(
                          '$page',
                          style: const TextStyle(
                            fontWeight:
                            FontWeight.w700,
                          ),
                        ),
                      ),
                    ),
                  );
                },
              ),

              // NEXT
              IconButton(
                onPressed: currentPage < totalPages
                    ? () => _changePage(
                  currentPage + 1,
                )
                    : null,
                icon: const Icon(
                  Icons.chevron_right,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
// ============================================================
// PRODUCT CARD
// ============================================================

  Widget _buildProductCard(
      ProductModel product,
      ) {
    final bool inStock =
        product.stockQuantity > 0;

    final String? image =
    product.images.isNotEmpty
        ? product.images.first
        : null;

    return GestureDetector(
      onTap: () {
// TODO:
// Navigate to Product Detail
      },

      child:
      Container(
        clipBehavior:
        Clip.antiAlias,

        decoration:
        BoxDecoration(
          color:
          Colors.white,

          borderRadius:
          BorderRadius.circular(
              18),

          border:
          Border.all(
            color:
            const Color(
                0xFFE5E7EB),
          ),
        ),

        child:
        Column(
          crossAxisAlignment:
          CrossAxisAlignment.start,

          children: [

// ==================================================
// IMAGE
// ==================================================

            Expanded(
              child:
              Stack(
                children: [

                  Positioned.fill(
                    child:
                    image != null
                        ? Image.network(
                      image,

                      fit:
                      BoxFit.cover,

                      errorBuilder:
                          (_, __, ___) {
                        return _noImage();
                      },
                    )
                        : _noImage(),
                  ),

// STOCK BADGE
                  Positioned(
                    top:
                    10,

                    right:
                    10,

                    child:
                    Container(
                      padding:
                      const EdgeInsets
                          .symmetric(
                        horizontal:
                        8,

                        vertical:
                        4,
                      ),

                      decoration:
                      BoxDecoration(
                        color:
                        inStock
                            ? const Color(
                            0xFFDCFCE7)
                            : const Color(
                            0xFFFEE2E2),

                        borderRadius:
                        BorderRadius.circular(
                            20),
                      ),

                      child:
                      Text(
                        inStock
                            ? 'In stock'
                            : 'Out of stock',

                        style:
                        TextStyle(
                          fontSize:
                          9,

                          fontWeight:
                          FontWeight.w800,

                          color:
                          inStock
                              ? const Color(
                              0xFF166534)
                              : const Color(
                              0xFF991B1B),
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),

// ==================================================
// PRODUCT INFO
// ==================================================

            Padding(
              padding:
              const EdgeInsets.all(
                  12),

              child:
              Column(
                crossAxisAlignment:
                CrossAxisAlignment.start,

                children: [

// CATEGORY
                  if (product.categoryName !=
                      null)
                    Text(
                      product.categoryName!,

                      maxLines:
                      1,

                      overflow:
                      TextOverflow.ellipsis,

                      style:
                      const TextStyle(
                        fontSize:
                        10,

                        fontWeight:
                        FontWeight.w800,

                        color:
                        Color(
                            0xFF166534),
                      ),
                    ),

                  const SizedBox(
                    height:
                    4,
                  ),

// BRAND
                  if (product.brandName !=
                      null)
                    Text(
                      product.brandName!,

                      maxLines:
                      1,

                      overflow:
                      TextOverflow.ellipsis,

                      style:
                      const TextStyle(
                        fontSize:
                        10,

                        fontWeight:
                        FontWeight.w700,

                        color:
                        Color(
                            0xFF1D4ED8),
                      ),
                    ),

                  const SizedBox(
                    height:
                    6,
                  ),

// NAME
                  Text(
                    product.productName,

                    maxLines:
                    2,

                    overflow:
                    TextOverflow.ellipsis,

                    style:
                    const TextStyle(
                      fontSize:
                      14,

                      fontWeight:
                      FontWeight.w800,
                    ),
                  ),

                  const SizedBox(
                    height:
                    8,
                  ),

// PRICE
                  Text(
                    '${product.productPrice.toStringAsFixed(0)}₫',

                    style:
                    const TextStyle(
                      fontSize:
                      17,

                      fontWeight:
                      FontWeight.w900,

                      color:
                      Color(
                          0xFF27AE60),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

// ============================================================
// NO IMAGE
// ============================================================

  Widget _noImage() {
    return Container(
      color:
      const Color(
          0xFFF0FDF4),

      child:
      const Center(
        child:
        Text(
          '🐾',

          style:
          TextStyle(
            fontSize:
            45,
          ),
        ),
      ),
    );
  }

// ============================================================
// EMPTY STATE
// ============================================================

  Widget _buildEmptyState() {
    return Center(
      child:
      Column(
        mainAxisAlignment:
        MainAxisAlignment.center,

        children: [

          const Text(
            '🔍',

            style:
            TextStyle(
              fontSize:
              60,
            ),
          ),

          const SizedBox(
            height:
            16,
          ),

          const Text(
            'No products found',

            style:
            TextStyle(
              fontSize:
              20,

              fontWeight:
              FontWeight.w800,
            ),
          ),

          const SizedBox(
            height:
            8,
          ),

          const Text(
            'Try changing filters or search keywords',

            style:
            TextStyle(
              color:
              Color(
                  0xFF6B7280),
            ),
          ),

          const SizedBox(
            height:
            16,
          ),

          OutlinedButton(
            onPressed:
            _resetFilter,

            child:
            const Text(
              'Clear Filters',
            ),
          ),
        ],
      ),
    );
  }

// ============================================================
// ERROR
// ============================================================

  Widget _buildError() {
    return Center(
      child:
      Padding(
        padding:
        const EdgeInsets.all(
            24),

        child:
        Column(
          mainAxisAlignment:
          MainAxisAlignment.center,

          children: [

            const Icon(
              Icons.error_outline,

              size:
              60,

              color:
              Colors.red,
            ),

            const SizedBox(
              height:
              16,
            ),

            Text(
              errorMessage ??
                  'Something went wrong',

              textAlign:
              TextAlign.center,
            ),

            const SizedBox(
              height:
              16,
            ),

            ElevatedButton(
              onPressed:
              _loadData,

              child:
              const Text(
                'Try Again',
              ),
            ),
          ],
        ),
      ),
    );
  }

// ============================================================
// DISPOSE
// ============================================================

  @override
  void dispose() {
    _searchController.dispose();

    super.dispose();
  }
}

// ================================================================
// FEATURE CHIP
// ================================================================

class _FeatureChip
    extends StatelessWidget {
  final String text;

  const _FeatureChip({
    required this.text,
  });

  @override
  Widget build(
      BuildContext context,
      ) {
    return Container(
      padding:
      const EdgeInsets.symmetric(
        horizontal:
        14,

        vertical:
        8,
      ),

      decoration:
      BoxDecoration(
        color:
        const Color(
            0xFFF8FAFC),

        borderRadius:
        BorderRadius.circular(
            30),

        border:
        Border.all(
          color:
          const Color(
              0xFFE5E7EB),
        ),
      ),

      child:
      Text(
        text,

        style:
        const TextStyle(
          fontSize:
          12,

          fontWeight:
          FontWeight.w600,

          color:
          Color(
              0xFF374151),
        ),
      ),
    );
  }
}
