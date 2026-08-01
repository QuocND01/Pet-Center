class AvailableSlotModel {
  final DateTime startTime;
  final DateTime endTime;
  final int gapBeforeMinutes;
  final int gapAfterMinutes;
  final int score;
  final bool isRecommended;
  final int? recommendationRank;

  AvailableSlotModel({
    required this.startTime,
    required this.endTime,
    required this.gapBeforeMinutes,
    required this.gapAfterMinutes,
    required this.score,
    required this.isRecommended,
    this.recommendationRank,
  });

  factory AvailableSlotModel.fromJson(Map<String, dynamic> json) {
    return AvailableSlotModel(
      startTime: DateTime.parse(json['startTime'] ?? json['StartTime']),
      endTime: DateTime.parse(json['endTime'] ?? json['EndTime']),
      gapBeforeMinutes: json['gapBeforeMinutes'] ?? json['GapBeforeMinutes'] ?? 0,
      gapAfterMinutes: json['gapAfterMinutes'] ?? json['GapAfterMinutes'] ?? 0,
      score: json['score'] ?? json['Score'] ?? 0,
      isRecommended: json['isRecommended'] ?? json['IsRecommended'] ?? false,
      recommendationRank: json['recommendationRank'] ?? json['RecommendationRank'],
    );
  }
}