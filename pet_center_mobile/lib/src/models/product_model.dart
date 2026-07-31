class ProductModel {
  final String productId;
  final String productName;
  final double productPrice;
  final String? productDescription;
  final int stockQuantity;
  final DateTime? addedAt;

  final String? brandName;
  final String? brandLogo;

  final String? categoryName;

  final List<String> images;

  ProductModel({
    required this.productId,
    required this.productName,
    required this.productPrice,
    this.productDescription,
    required this.stockQuantity,
    this.addedAt,
    this.brandName,
    this.brandLogo,
    this.categoryName,
    required this.images,
  });

  factory ProductModel.fromJson(Map<String, dynamic> json) {
    final rawImages = json['Images'] ?? json['images'];

    List<String> parsedImages = [];

    if (rawImages is List) {
      parsedImages = rawImages
          .map((e) => e.toString())
          .toList();
    }

    return ProductModel(
      productId:
      (json['ProductId'] ?? json['productId'] ?? '')
          .toString(),

      productName:
      (json['ProductName'] ?? json['productName'] ?? '')
          .toString(),

      productPrice:
      (json['ProductPrice'] ?? json['productPrice'] ?? 0)
      is num
          ? (json['ProductPrice'] ?? json['productPrice'] ?? 0)
          .toDouble()
          : 0.0,

      productDescription:
      json['ProductDescription'] ??
          json['productDescription'],

      stockQuantity:
      (json['StockQuantity'] ??
          json['stockQuantity'] ??
          0)
      as int,

      addedAt: DateTime.tryParse(
        (json['AddedAt'] ??
            json['addedAt'] ??
            '')
            .toString(),
      ),

      brandName:
      json['BrandName'] ??
          json['brandName'],

      brandLogo:
      json['BrandLogo'] ??
          json['brandLogo'],

      categoryName:
      json['CategoryName'] ??
          json['categoryName'],

      images: parsedImages,
    );
  }
}