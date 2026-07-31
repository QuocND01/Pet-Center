class CategoryModel {
  final String categoryId;
  final String categoryName;

  CategoryModel({
    required this.categoryId,
    required this.categoryName,
  });

  factory CategoryModel.fromJson(
      Map<String, dynamic> json,
      ) {
    return CategoryModel(
      categoryId:
      json['CategoryId']?.toString() ?? '',
      categoryName:
      json['CategoryName']?.toString() ?? '',
    );
  }
}