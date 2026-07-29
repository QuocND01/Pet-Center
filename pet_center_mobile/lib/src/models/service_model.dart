class ServiceModel {
  final String serviceId;
  final String serviceName;
  final double price;
  final String? serviceDescription;
  final int duration;
  final int serviceType; // 1 = Veterinary, 2 = Grooming
  final List<String> imageFiles;

  ServiceModel({
    required this.serviceId,
    required this.serviceName,
    required this.price,
    this.serviceDescription,
    required this.duration,
    required this.serviceType,
    required this.imageFiles,
  });

  factory ServiceModel.fromJson(Map<String, dynamic> json) {
    var imagesJson = json['imageFiles'] ?? json['ImageFiles'] ?? json['imageUrls'] ?? json['ImageUrls'];
    List<String> images = [];
    if (imagesJson is List) {
      images = List<String>.from(imagesJson.map((x) => x.toString()));
    }

    return ServiceModel(
      serviceId: json['serviceId'] ?? json['ServiceId'] ?? '',
      serviceName: json['serviceName'] ?? json['ServiceName'] ?? 'Unnamed Service',
      price: (json['price'] ?? json['Price'] ?? 0).toDouble(),
      serviceDescription: json['serviceDescription'] ?? json['ServiceDescription'],
      duration: json['duration'] ?? json['Duration'] ?? 0,
      serviceType: json['serviceType'] ?? json['ServiceType'] ?? 0,
      imageFiles: images,
    );
  }

  String get typeName {
    switch (serviceType) {
      case 1:
        return 'Veterinary';
      case 2:
        return 'Grooming';
      default:
        return 'Pet Care';
    }
  }

  String get typeIcon {
    switch (serviceType) {
      case 1:
        return '🩺';
      case 2:
        return '✂️';
      default:
        return '🐾';
    }
  }
}
