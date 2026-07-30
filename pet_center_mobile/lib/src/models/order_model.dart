class OrderItemModel {
  final String productId;
  final String productName;
  final String? productCategory;
  final String? productBrand;
  final String? productImage;
  final int quantity;
  final double unitPrice;
  final double subTotal;

  OrderItemModel({
    required this.productId,
    required this.productName,
    this.productCategory,
    this.productBrand,
    this.productImage,
    required this.quantity,
    required this.unitPrice,
    required this.subTotal,
  });

  factory OrderItemModel.fromJson(Map<String, dynamic> json) {
    final qty = json['quantity'] ?? json['Quantity'] ?? 1;
    final price = (json['unitPrice'] ?? json['UnitPrice'] ?? 0.0).toDouble();

    return OrderItemModel(
      productId: json['productId'] ?? json['ProductId'] ?? '',
      productName: json['productName'] ?? json['ProductName'] ?? '',
      productCategory: json['productCategory'] ?? json['ProductCategory'],
      productBrand: json['productBrand'] ?? json['ProductBrand'],
      productImage: json['productImage'] ?? json['ProductImage'],
      quantity: qty,
      unitPrice: price,
      subTotal: (json['subTotal'] ?? json['SubTotal'] ?? (qty * price)).toDouble(),
    );
  }
}

class OrderModel {
  final String orderId;
  final String customerName;
  final String phoneNumber;
  final DateTime orderDate;
  final DateTime? deliveredDate;
  final double totalAmount;
  final double? discountAmount;
  final int status; // 0 = Cancelled, 1 = Pending, 2 = Processing, 3 = Shipping, 4 = Completed
  final String paymentMethod;
  final int paymentStatus;
  final String addressSnapshot;
  final String? email;
  final List<OrderItemModel> orderItems;

  OrderModel({
    required this.orderId,
    required this.customerName,
    required this.phoneNumber,
    required this.orderDate,
    this.deliveredDate,
    required this.totalAmount,
    this.discountAmount,
    required this.status,
    required this.paymentMethod,
    required this.paymentStatus,
    required this.addressSnapshot,
    this.email,
    required this.orderItems,
  });

  factory OrderModel.fromJson(Map<String, dynamic> json) {
    DateTime parseDate(dynamic val) {
      if (val == null) return DateTime.now();
      try {
        return DateTime.parse(val.toString());
      } catch (_) {
        return DateTime.now();
      }
    }

    DateTime? parseOptionalDate(dynamic val) {
      if (val == null) return null;
      try {
        return DateTime.parse(val.toString());
      } catch (_) {
        return null;
      }
    }

    var rawItems = json['orderItems'] ?? json['OrderItems'];
    List<OrderItemModel> items = [];
    if (rawItems is List) {
      items = rawItems.map((i) => OrderItemModel.fromJson(i)).toList();
    }

    return OrderModel(
      orderId: json['orderId'] ?? json['OrderId'] ?? '',
      customerName: json['customerName'] ?? json['CustomerName'] ?? 'Customer',
      phoneNumber: json['phoneNumber'] ?? json['PhoneNumber'] ?? '',
      orderDate: parseDate(json['orderDate'] ?? json['OrderDate']),
      deliveredDate: parseOptionalDate(json['deliveredDate'] ?? json['DeliveredDate']),
      totalAmount: (json['totalAmount'] ?? json['TotalAmount'] ?? 0.0).toDouble(),
      discountAmount: json['discountAmount'] != null ? (json['discountAmount']).toDouble() : null,
      status: json['status'] ?? json['Status'] ?? 1,
      paymentMethod: json['paymentMethod'] ?? json['PaymentMethod'] ?? 'COD',
      paymentStatus: json['paymentStatus'] ?? json['PaymentStatus'] ?? 0,
      addressSnapshot: json['addressSnapshot'] ?? json['AddressSnapshot'] ?? '',
      email: json['email'] ?? json['Email'],
      orderItems: items,
    );
  }

  String get statusText {
    switch (status) {
      case 0:
        return 'Cancelled';
      case 1:
        return 'Pending';
      case 2:
        return 'Processing';
      case 3:
        return 'Delivering';
      case 4:
        return 'Completed';
      default:
        return 'Unknown';
    }
  }
}
