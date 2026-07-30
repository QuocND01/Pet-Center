class FeedbackMediaModel {
  final String mediaId;
  final String mediaUrl;
  final String? publicId;
  final String mediaType; // "image" or "video"

  FeedbackMediaModel({
    required this.mediaId,
    required this.mediaUrl,
    this.publicId,
    required this.mediaType,
  });

  factory FeedbackMediaModel.fromJson(Map<String, dynamic> json) {
    return FeedbackMediaModel(
      mediaId: json['mediaId'] ?? json['MediaId'] ?? '',
      mediaUrl: json['mediaUrl'] ?? json['MediaUrl'] ?? '',
      publicId: json['publicId'] ?? json['PublicId'],
      mediaType: json['mediaType'] ?? json['MediaType'] ?? 'image',
    );
  }
}

class ProductFeedbackModel {
  final String feedbackId;
  final String customerId;
  final String? customerName;
  final String productId;
  final String orderId;
  final int rating;
  final String? comment;
  final String? reply;
  final DateTime? replyDate;
  final DateTime? createdDate;
  final DateTime? updatedAt;
  final List<FeedbackMediaModel> mediaFiles;

  ProductFeedbackModel({
    required this.feedbackId,
    required this.customerId,
    this.customerName,
    required this.productId,
    required this.orderId,
    required this.rating,
    this.comment,
    this.reply,
    this.replyDate,
    this.createdDate,
    this.updatedAt,
    required this.mediaFiles,
  });

  factory ProductFeedbackModel.fromJson(Map<String, dynamic> json) {
    var rawMedia = json['mediaFiles'] ?? json['MediaFiles'];
    List<FeedbackMediaModel> mediaList = [];
    if (rawMedia is List) {
      mediaList = rawMedia.map((m) => FeedbackMediaModel.fromJson(m)).toList();
    }

    DateTime? parseDate(dynamic val) {
      if (val == null) return null;
      try {
        return DateTime.parse(val.toString());
      } catch (_) {
        return null;
      }
    }

    return ProductFeedbackModel(
      feedbackId: json['feedbackId'] ?? json['FeedbackId'] ?? '',
      customerId: json['customerId'] ?? json['CustomerId'] ?? '',
      customerName: json['customerName'] ?? json['CustomerName'],
      productId: json['productId'] ?? json['ProductId'] ?? '',
      orderId: json['orderId'] ?? json['OrderId'] ?? '',
      rating: json['rating'] ?? json['Rating'] ?? 5,
      comment: json['comment'] ?? json['Comment'],
      reply: json['reply'] ?? json['Reply'],
      replyDate: parseDate(json['replyDate'] ?? json['ReplyDate']),
      createdDate: parseDate(json['createdDate'] ?? json['CreatedDate']),
      updatedAt: parseDate(json['updatedAt'] ?? json['UpdatedAt']),
      mediaFiles: mediaList,
    );
  }
}
