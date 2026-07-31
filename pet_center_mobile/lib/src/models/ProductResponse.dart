import 'package:pet_center_mobile/src/models/product_model.dart';

class ProductResponse {
  final List<ProductModel> products;
  final int count;

  ProductResponse({
    required this.products,
    required this.count,
  });

  factory ProductResponse.fromJson(Map<String, dynamic> json) {
    final rawValues = json['value'];

    final List<ProductModel> products = [];

    if (rawValues is List) {
      for (final item in rawValues) {
        if (item is Map<String, dynamic>) {
          products.add(
            ProductModel.fromJson(item),
          );
        }
      }
    }

    return ProductResponse(
      products: products,
      count: (json['@odata.count'] as num?)?.toInt() ?? 0,
    );
  }
}