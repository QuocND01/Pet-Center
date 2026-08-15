class BookingPetModel {
  final String petId;
  final String petName;
  final String? species;
  final String? breed;
  final String? gender;
  final double? weight;
  final String? petAvatar;

  BookingPetModel({
    required this.petId,
    required this.petName,
    this.species,
    this.breed,
    this.gender,
    this.weight,
    this.petAvatar,
  });

  factory BookingPetModel.fromJson(Map<String, dynamic> json) {
    return BookingPetModel(
      petId: json['petId'] ?? json['PetId'] ?? '',
      petName: json['petName'] ?? json['PetName'] ?? '',
      species: json['species'] ?? json['Species'],
      breed: json['breed'] ?? json['Breed'],
      gender: json['gender'] ?? json['Gender'],
      weight: (json['weight'] ?? json['Weight']) != null
          ? (json['weight'] ?? json['Weight']).toDouble()
          : null,
      petAvatar: json['petAvatar'] ?? json['PetAvatar'],
    );
  }
}

class BookingStaffModel {
  final String staffId;
  final String fullName;
  final String? avatar;
  final String phoneNumber;
  final String email;
  final double? experienceYears;
  final String? description;
  final String? licenseNumber;
  final String role; // 'Vet' hoặc 'Groomer'

  BookingStaffModel({
    required this.staffId,
    required this.fullName,
    this.avatar,
    required this.phoneNumber,
    required this.email,
    this.experienceYears,
    this.description,
    this.licenseNumber,
    required this.role,
  });

  factory BookingStaffModel.fromJson(Map<String, dynamic> json) {
    return BookingStaffModel(
      staffId: json['staffId'] ?? json['StaffId'] ?? '',
      fullName: json['fullName'] ?? json['FullName'] ?? '',
      avatar: json['avatar'] ?? json['Avatar'],
      phoneNumber: json['phoneNumber'] ?? json['PhoneNumber'] ?? '',
      email: json['email'] ?? json['Email'] ?? '',
      experienceYears: (json['experienceYears'] ?? json['ExperienceYears']) != null
          ? (json['experienceYears'] ?? json['ExperienceYears']).toDouble()
          : null,
      description: json['description'] ?? json['Description'],
      licenseNumber: json['licenseNumber'] ?? json['LicenseNumber'],
      role: json['role'] ?? json['Role'] ?? '',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'staffId': staffId,
      'fullName': fullName,
      'avatar': avatar,
      'phoneNumber': phoneNumber,
      'email': email,
      'experienceYears': experienceYears,
      'description': description,
      'licenseNumber': licenseNumber,
      'role': role,
    };
  }

  // --- Các Getter tiện ích hỗ trợ lọc logic ---
  bool get isVet => role.trim().toLowerCase() == 'vet';
  bool get isGroomer => role.trim().toLowerCase() == 'groomer';

  /// Kiểm tra xem 1 dịch vụ (serviceType: 1 là Y tế, 2 là Grooming) có phù hợp chuyên môn không
  bool canHandleService(int serviceType) {
    if (isVet && serviceType == 1) return true;
    if (isGroomer && serviceType == 2) return true;
    return false;
  }
}

class BookingServiceModel {
  final String serviceId;
  final String serviceName;
  final double price;
  final int duration;
  final int serviceType;
  final List<String> serviceImages;

  BookingServiceModel({
    required this.serviceId,
    required this.serviceName,
    required this.price,
    required this.duration,
    required this.serviceType,
    required this.serviceImages,
  });

  factory BookingServiceModel.fromJson(Map<String, dynamic> json) {
    final rawImages = json['serviceImages'] ?? json['ServiceImages'];
    List<String> parsedImages = [];
    if (rawImages is List) {
      parsedImages = rawImages.map((e) => e.toString()).toList();
    }

    return BookingServiceModel(
      serviceId: json['serviceId'] ?? json['ServiceId'] ?? '',
      serviceName: json['serviceName'] ?? json['ServiceName'] ?? '',
      price: ((json['price'] ?? json['Price']) ?? 0).toDouble(),
      duration: (json['duration'] ?? json['Duration']) ?? 0,
      serviceType: (json['serviceType'] ?? json['ServiceType']) ?? 0,
      serviceImages: parsedImages,
    );
  }
}

class BookingPageModel {
  final String? petId;
  final String? staffId;
  final List<String> serviceIds;
  final DateTime? appointmentStart;
  final String? note;
  final List<BookingPetModel> pets;
  final List<BookingStaffModel> staffs;
  final List<BookingServiceModel> services;

  BookingPageModel({
    this.petId,
    this.staffId,
    this.serviceIds = const [],
    this.appointmentStart,
    this.note,
    this.pets = const [],
    this.staffs = const [],
    this.services = const [],
  });

  factory BookingPageModel.fromJson(Map<String, dynamic> json) {
    final rawPets = json['pets'] ?? json['Pets'];
    final rawStaffs = json['staffs'] ?? json['Staffs'];
    final rawServices = json['services'] ?? json['Services'];
    final rawServiceIds = json['serviceIds'] ?? json['ServiceIds'];

    return BookingPageModel(
      petId: json['petId'] ?? json['PetId'],
      staffId: json['staffId'] ?? json['StaffId'],
      note: json['note'] ?? json['Note'],
      appointmentStart: (json['appointmentStart'] ?? json['AppointmentStart']) != null
          ? DateTime.tryParse(json['appointmentStart'] ?? json['AppointmentStart'])
          : null,
      serviceIds: rawServiceIds is List
          ? rawServiceIds.map((e) => e.toString()).toList()
          : <String>[],
      pets: rawPets is List
          ? rawPets.map((x) => BookingPetModel.fromJson(x as Map<String, dynamic>)).toList()
          : <BookingPetModel>[],
      staffs: rawStaffs is List
          ? rawStaffs.map((x) => BookingStaffModel.fromJson(x as Map<String, dynamic>)).toList()
          : <BookingStaffModel>[],
      services: rawServices is List
          ? rawServices.map((x) => BookingServiceModel.fromJson(x as Map<String, dynamic>)).toList()
          : <BookingServiceModel>[],
    );
  }
}