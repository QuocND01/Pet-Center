class CategoryAttributeModel {
  final String categoryAttributeId;
  final String attributeName;

  CategoryAttributeModel({
    required this.categoryAttributeId,
    required this.attributeName,
  });

  factory CategoryAttributeModel.fromJson(
      Map<String, dynamic> json,
      ) {
    return CategoryAttributeModel(
      categoryAttributeId:
      json['categoryAttributeId'] ?? '',
      attributeName:
      json['attributeName'] ?? '',
    );
  }
}