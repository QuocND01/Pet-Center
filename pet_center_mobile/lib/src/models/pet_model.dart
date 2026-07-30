class PetModel {
  final String petId;
  final String petName;
  final String species;
  final String breed;
  final String gender;
  final String? petAvatar;
  final double? weight;
  final String? note;
  final String? dateOfBirth;
  final bool isActive;

  PetModel({
    required this.petId,
    required this.petName,
    required this.species,
    required this.breed,
    required this.gender,
    this.petAvatar,
    this.weight,
    this.note,
    this.dateOfBirth,
    this.isActive = true,
  });

  factory PetModel.fromJson(Map<String, dynamic> json) {
    return PetModel(
      petId: json['petId'] ?? json['PetId'] ?? '',
      petName: json['petName'] ?? json['PetName'] ?? '',
      species: json['species'] ?? json['Species'] ?? '',
      breed: json['breed'] ?? json['Breed'] ?? '',
      gender: json['gender'] ?? json['Gender'] ?? '',
      petAvatar: json['petAvatar'] ?? json['PetAvatar'],
      weight: json['weight'] != null
          ? (json['weight'] as num).toDouble()
          : (json['Weight'] != null
              ? (json['Weight'] as num).toDouble()
              : null),
      note: json['note'] ?? json['Note'],
      dateOfBirth: json['dateOfBirth'] ?? json['DateOfBirth'],
      isActive: json['isActive'] ?? json['IsActive'] ?? true,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'petId': petId,
      'petName': petName,
      'species': species,
      'breed': breed,
      'gender': gender,
      'petAvatar': petAvatar,
      'weight': weight,
      'note': note,
      'dateOfBirth': dateOfBirth,
      'isActive': isActive,
    };
  }

  String? getFullAvatarUrl(String baseUrl) {
    if (petAvatar == null || petAvatar!.isEmpty) return null;
    if (petAvatar!.startsWith('http://') || petAvatar!.startsWith('https://')) {
      return petAvatar;
    }
    final cleanBase = baseUrl.endsWith('/api')
        ? baseUrl.substring(0, baseUrl.length - 4)
        : baseUrl;
    final cleanPath = petAvatar!.startsWith('/') ? petAvatar : '/$petAvatar';
    return '$cleanBase$cleanPath';
  }

  String get ageDisplay {
    if (dateOfBirth == null || dateOfBirth!.isEmpty) {
      return 'Age not updated';
    }
    try {
      final dob = DateTime.parse(dateOfBirth!);
      final now = DateTime.now();
      int years = now.year - dob.year;
      int months = now.month - dob.month;
      if (now.day < dob.day) {
        months--;
      }
      if (months < 0) {
        years--;
        months += 12;
      }
      if (years > 0) {
        return months > 0
            ? '$years yrs $months mos'
            : '$years ${years == 1 ? "year" : "years"} old';
      } else if (months > 0) {
        return '$months ${months == 1 ? "month" : "months"} old';
      } else {
        return 'Under 1 month old';
      }
    } catch (_) {
      return dateOfBirth!;
    }
  }
}
