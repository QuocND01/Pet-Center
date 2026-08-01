import 'dart:io';

class ProductFeedbackInput {
  final String productId;
  final String productName;
  final String? productImage;
  final String orderId;

  int rating;
  String comment;
  List<File> imageFiles;
  File? videoFile;

  ProductFeedbackInput({
    required this.productId,
    required this.productName,
    this.productImage,
    required this.orderId,
    this.rating = 5,
    this.comment = '',
    List<File>? imageFiles,
    this.videoFile,
  }) : imageFiles = imageFiles ?? [];
}
