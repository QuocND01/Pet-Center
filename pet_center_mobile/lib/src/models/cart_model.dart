import 'product_model.dart';

class CartResponseModel {
  final String cartId;
  final String customerId;
  final List<CartDetailModel> cartDetails;

  CartResponseModel({
    required this.cartId,
    required this.customerId,
    required this.cartDetails,
  });

  factory CartResponseModel.fromJson(Map<String, dynamic> json) {
    var details = json['cartDetails'] ?? json['CartDetails'] ?? [];
    List<CartDetailModel> parsedDetails = [];
    if (details is List) {
      parsedDetails = details.map((d) => CartDetailModel.fromJson(d)).toList();
    }

    return CartResponseModel(
      cartId: json['cartId'] ?? json['CartId'] ?? '',
      customerId: json['customerId'] ?? json['CustomerId'] ?? '',
      cartDetails: parsedDetails,
    );
  }
}

class CartDetailModel {
  final String cartDetailId;
  final String cartId;
  final String productId;
  int quantity;
  ProductModel? product; // Tải động từ chi tiết sản phẩm trên API

  CartDetailModel({
    required this.cartDetailId,
    required this.cartId,
    required this.productId,
    required this.quantity,
    this.product,
  });

  factory CartDetailModel.fromJson(Map<String, dynamic> json) {
    return CartDetailModel(
      cartDetailId: json['cartDetailId'] ?? json['CartDetailId'] ?? '',
      cartId: json['cartId'] ?? json['CartId'] ?? '',
      productId: json['productId'] ?? json['ProductId'] ?? '',
      quantity: json['quantity'] ?? json['Quantity'] ?? 0,
    );
  }
}
