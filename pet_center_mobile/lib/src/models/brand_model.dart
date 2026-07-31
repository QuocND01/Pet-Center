class BrandModel {
  final String brandId;
  final String brandName;

  BrandModel({
    required this.brandId,
    required this.brandName,
  });

  factory BrandModel.fromJson(
      Map<String, dynamic> json,
      ) {
    return BrandModel(
      brandId: json['BrandId']?.toString() ?? '',
      brandName: json['BrandName']?.toString() ?? '',
    );
  }
}