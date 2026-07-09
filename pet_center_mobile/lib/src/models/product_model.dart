class ProductModel {
  final String productId;
  final String productName;
  final double productPrice;
  final String? productDescription;
  final int stockQuantity;
  final String? brandName;
  final String? categoryName;
  final List<String> images;

  ProductModel({
    required this.productId,
    required this.productName,
    required this.productPrice,
    this.productDescription,
    required this.stockQuantity,
    this.brandName,
    this.categoryName,
    required this.images,
  });

  factory ProductModel.fromJson(Map<String, dynamic> json) {
    var rawImages = json['images'] ?? json['Images'];
    List<String> parsedImages = [];
    if (rawImages is List) {
      parsedImages = rawImages.map((e) => e.toString()).toList();
    }

    return ProductModel(
      productId: json['productId'] ?? json['ProductId'] ?? '',
      productName: json['productName'] ?? json['ProductName'] ?? '',
      productPrice: (json['productPrice'] ?? json['ProductPrice'] ?? 0.0).toDouble(),
      productDescription: json['productDescription'] ?? json['ProductDescription'],
      stockQuantity: json['stockQuantity'] ?? json['StockQuantity'] ?? 0,
      brandName: json['brandName'] ?? json['BrandName'],
      categoryName: json['categoryName'] ?? json['CategoryName'],
      images: parsedImages,
    );
  }
}
