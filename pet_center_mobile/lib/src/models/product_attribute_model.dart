class ProductAttributeModel {
  final String categoryAttributeId;
  final String attributeName;
  final String? attributeValue;

  ProductAttributeModel({
    required this.categoryAttributeId,
    required this.attributeName,
    this.attributeValue,
  });

  factory ProductAttributeModel.fromJson(
      Map<String, dynamic> json,
      ) {
    return ProductAttributeModel(
      categoryAttributeId:
      json['categoryAttributeId'] ?? '',

      attributeName:
      json['attributeName'] ?? '',

      attributeValue:
      json['attributeValue'],
    );
  }
}