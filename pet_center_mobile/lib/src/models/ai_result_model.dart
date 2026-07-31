class AIResultModel {
  final String? diseaseId;
  final String diseaseName;
  final double confidence;
  final String? description;
  final String? recommendation;
  final int? species;
  final bool isDiseaseImage;
  final bool hasDiseaseInfo;
  final String? message;

  AIResultModel({
    this.diseaseId,
    required this.diseaseName,
    required this.confidence,
    this.description,
    this.recommendation,
    this.species,
    required this.isDiseaseImage,
    required this.hasDiseaseInfo,
    this.message,
  });

  factory AIResultModel.fromJson(
      Map<String, dynamic> json,
      ) {
    return AIResultModel(
      diseaseId:
      json['diseaseId'],

      diseaseName:
      json['diseaseName'] ?? '',

      confidence:
      (json['confidence'] ?? 0).toDouble(),

      description:
      json['description'],

      recommendation:
      json['recommendation'],

      species:
      json['species'],

      isDiseaseImage:
      json['isDiseaseImage'] ?? false,

      hasDiseaseInfo:
      json['hasDiseaseInfo'] ?? false,

      message:
      json['message'],
    );
  }
}

class AIDiseaseInfoModel {
  final String diagnosis;
  final String treatment;

  AIDiseaseInfoModel({
    required this.diagnosis,
    required this.treatment,
  });

  factory AIDiseaseInfoModel.fromJson(
      Map<String, dynamic> json,
      ) {
    return AIDiseaseInfoModel(
      diagnosis:
      json['diagnosis'] ?? '',

      treatment:
      json['treatment'] ?? '',
    );
  }
}