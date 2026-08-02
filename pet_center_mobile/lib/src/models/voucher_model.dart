class VoucherModel {
  final String voucherId;
  final String code;
  final String? description;
  final int? discountPercent;
  final double? minOrderAmount;
  final double? maxDiscountAmount;
  final DateTime? expiredDate;

  VoucherModel({
    required this.voucherId,
    required this.code,
    this.description,
    this.discountPercent,
    this.minOrderAmount,
    this.maxDiscountAmount,
    this.expiredDate,
  });

  factory VoucherModel.fromJson(Map<String, dynamic> json) {
    return VoucherModel(
      voucherId: json['voucherId'] ?? json['VoucherId'] ?? '',
      code: json['code'] ?? json['Code'] ?? '',
      description: json['description'] ?? json['Description'],
      discountPercent: (json['discountPercent'] ?? json['DiscountPercent']) as int?,
      minOrderAmount: (json['minOrderAmount'] ?? json['MinOrderAmount']) != null
          ? (json['minOrderAmount'] ?? json['MinOrderAmount']).toDouble()
          : null,
      maxDiscountAmount: (json['maxDiscountAmount'] ?? json['MaxDiscountAmount']) != null
          ? (json['maxDiscountAmount'] ?? json['MaxDiscountAmount']).toDouble()
          : null,
      expiredDate: json['expiredDate'] != null || json['ExpiredDate'] != null
          ? DateTime.tryParse(json['expiredDate'] ?? json['ExpiredDate'] ?? '')
          : null,
    );
  }

  double calculateDiscount(double orderAmount) {
    if (minOrderAmount != null && orderAmount < minOrderAmount!) {
      return 0.0;
    }
    double discount = 0.0;
    if (discountPercent != null && discountPercent! > 0) {
      discount = orderAmount * (discountPercent! / 100.0);
    }
    if (maxDiscountAmount != null && maxDiscountAmount! > 0 && discount > maxDiscountAmount!) {
      discount = maxDiscountAmount!;
    }
    return discount;
  }
}
