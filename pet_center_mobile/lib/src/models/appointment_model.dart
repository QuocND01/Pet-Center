class AppointmentListModel {
  final String appointmentId;
  final String petName;
  final String vetName;
  final DateTime appointmentStart;
  final DateTime appointmentEnd;
  final int status;
  final double total;

  AppointmentListModel({
    required this.appointmentId,
    required this.petName,
    required this.vetName,
    required this.appointmentStart,
    required this.appointmentEnd,
    required this.status,
    required this.total,
  });

  factory AppointmentListModel.fromJson(Map<String, dynamic> json) {
    return AppointmentListModel(
      appointmentId: json['appointmentId'] ?? json['AppointmentId'] ?? '',
      petName: json['petName'] ?? json['PetName'] ?? '',
      vetName: json['vetName'] ?? json['VetName'] ?? '',
      appointmentStart: json['appointmentStart'] != null || json['AppointmentStart'] != null
          ? DateTime.parse(json['appointmentStart'] ?? json['AppointmentStart'])
          : DateTime.now(),
      appointmentEnd: json['appointmentEnd'] != null || json['AppointmentEnd'] != null
          ? DateTime.parse(json['appointmentEnd'] ?? json['AppointmentEnd'])
          : DateTime.now(),
      status: json['status'] ?? json['Status'] ?? 0,
      total: (json['total'] ?? json['Total'] ?? 0.0).toDouble(),
    );
  }

  String get statusText {
    switch (status) {
      case 1:
        return 'Reserved (Pending)';
      case 2:
        return 'Confirmed';
      case 3:
        return 'Completed';
      case 0:
        return 'Cancelled';
      default:
        return 'Unknown ($status)';
    }
  }
}

class AppointmentServiceModel {
  final String appointmentServiceId;
  final String serviceName;
  final double price;
  final int duration;
  final int? status;

  AppointmentServiceModel({
    required this.appointmentServiceId,
    required this.serviceName,
    required this.price,
    required this.duration,
    this.status,
  });

  factory AppointmentServiceModel.fromJson(Map<String, dynamic> json) {
    return AppointmentServiceModel(
      appointmentServiceId: json['appointmentServiceId'] ?? json['AppointmentServiceId'] ?? '',
      serviceName: json['serviceName'] ?? json['ServiceName'] ?? '',
      price: (json['price'] ?? json['Price'] ?? 0.0).toDouble(),
      duration: json['duration'] ?? json['Duration'] ?? 0,
      status: json['status'] ?? json['Status'],
    );
  }
}

class AppointmentSnapshotModel {
  final String species;
  final String breed;
  final String gender;
  final double weight;
  final String? feedback;
  final double rating;
  final String vetName;

  AppointmentSnapshotModel({
    required this.species,
    required this.breed,
    required this.gender,
    required this.weight,
    this.feedback,
    required this.rating,
    required this.vetName,
  });

  factory AppointmentSnapshotModel.fromJson(Map<String, dynamic> json) {
    return AppointmentSnapshotModel(
      species: json['species'] ?? json['Species'] ?? '',
      breed: json['breed'] ?? json['Breed'] ?? '',
      gender: json['gender'] ?? json['Gender'] ?? '',
      weight: (json['weight'] ?? json['Weight'] ?? 0.0).toDouble(),
      feedback: json['feedback'] ?? json['Feedback'],
      rating: (json['rating'] ?? json['Rating'] ?? 0.0).toDouble(),
      vetName: json['vetName'] ?? json['VetName'] ?? '',
    );
  }
}

class AppointmentDetailModel {
  final String appointmentId;
  final String customerId;
  final String customerName;
  final String petId;
  final String staffId;
  final String petName;
  final String petAvatar;
  final String vetName;
  final String vetAvatar;
  final DateTime appointmentStart;
  final DateTime appointmentEnd;
  final double total;
  final int status;
  final String? note;
  final List<AppointmentServiceModel> appointmentServices;
  final AppointmentSnapshotModel? snapshot;

  AppointmentDetailModel({
    required this.appointmentId,
    required this.customerId,
    required this.customerName,
    required this.petId,
    required this.staffId,
    required this.petName,
    required this.petAvatar,
    required this.vetName,
    required this.vetAvatar,
    required this.appointmentStart,
    required this.appointmentEnd,
    required this.total,
    required this.status,
    this.note,
    required this.appointmentServices,
    this.snapshot,
  });

  factory AppointmentDetailModel.fromJson(Map<String, dynamic> json) {
    var servicesJson = json['appointmentServices'] ?? json['AppointmentServices'] ?? [];
    List<AppointmentServiceModel> parsedServices = [];
    if (servicesJson is List) {
      parsedServices = servicesJson.map((s) => AppointmentServiceModel.fromJson(s)).toList();
    }

    var snapshotJson = json['snapshot'] ?? json['Snapshot'];

    return AppointmentDetailModel(
      appointmentId: json['appointmentId'] ?? json['AppointmentId'] ?? '',
      customerId: json['customerId'] ?? json['CustomerId'] ?? '',
      customerName: json['customerName'] ?? json['CustomerName'] ?? '',
      petId: json['petId'] ?? json['PetId'] ?? '',
      staffId: json['staffId'] ?? json['StaffId'] ?? '',
      petName: json['petName'] ?? json['PetName'] ?? '',
      petAvatar: json['petAvatar'] ?? json['PetAvatar'] ?? '',
      vetName: json['vetName'] ?? json['VetName'] ?? '',
      vetAvatar: json['vetAvatar'] ?? json['VetAvatar'] ?? '',
      appointmentStart: json['appointmentStart'] != null || json['AppointmentStart'] != null
          ? DateTime.parse(json['appointmentStart'] ?? json['AppointmentStart'])
          : DateTime.now(),
      appointmentEnd: json['appointmentEnd'] != null || json['AppointmentEnd'] != null
          ? DateTime.parse(json['appointmentEnd'] ?? json['AppointmentEnd'])
          : DateTime.now(),
      total: (json['total'] ?? json['Total'] ?? 0.0).toDouble(),
      status: json['status'] ?? json['Status'] ?? 0,
      note: json['note'] ?? json['Note'],
      appointmentServices: parsedServices,
      snapshot: snapshotJson != null ? AppointmentSnapshotModel.fromJson(snapshotJson) : null,
    );
  }

  String get statusText {
    switch (status) {
      case 1:
        return 'Reserved (Pending)';
      case 2:
        return 'Confirmed';
      case 3:
        return 'Completed';
      case 0:
        return 'Cancelled';
      default:
        return 'Unknown ($status)';
    }
  }
}

class SubmitAppointmentReviewRequest {
  final String appointmentId;
  final int rating; // 1 to 5
  final String? reviewNote;

  SubmitAppointmentReviewRequest({
    required this.appointmentId,
    required this.rating,
    this.reviewNote,
  });

  Map<String, dynamic> toJson() => {
        'appointmentId': appointmentId,
        'rating': rating,
        'reviewNote': reviewNote,
      };
}

class UpdateAppointmentRequest {
  final String appointmentId;
  final String? petId;
  final String? staffId;
  final List<String> serviceIds;
  final DateTime appointmentStart;
  final String? note;

  UpdateAppointmentRequest({
    required this.appointmentId,
    this.petId,
    this.staffId,
    required this.serviceIds,
    required this.appointmentStart,
    this.note,
  });

  Map<String, dynamic> toJson() => {
        'appointmentId': appointmentId,
        'petId': petId,
        'staffId': staffId,
        'serviceIds': serviceIds,
        'appointmentStart': appointmentStart.toIso8601String(),
        'note': note,
      };
}
