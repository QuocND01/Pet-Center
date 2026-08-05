class CustomerModel {
  final String customerId;
  final String? fullName;
  final String? email;
  final String? phoneNumber;
  final String? birthDay; // Phù hợp với định dạng DateOnly (YYYY-MM-DD) từ C#
  final String? gender;
  final bool? isVerified;
  final bool? isActive;
  final DateTime? createdAt;

  CustomerModel({
    required this.customerId,
    this.fullName,
    this.email,
    this.phoneNumber,
    this.birthDay,
    this.gender,
    this.isVerified,
    this.isActive,
    this.createdAt,
  });

  // Chuyển đổi dữ liệu JSON trả về từ api/customer/profile
  factory CustomerModel.fromJson(Map<String, dynamic> json) {
    String? rawBirthday = json['birthDay'] ?? json['BirthDay'];
    if (rawBirthday != null && rawBirthday.trim().isNotEmpty) {
      if (rawBirthday.contains('T')) {
        rawBirthday = rawBirthday.split('T')[0];
      }
      rawBirthday = rawBirthday.trim();
    } else {
      rawBirthday = null;
    }

    String? phone = json['phoneNumber'] ?? json['PhoneNumber'];
    if (phone != null && (phone.trim().isEmpty || phone == 'Not updated')) {
      phone = null;
    }

    String? gen = json['gender'] ?? json['Gender'];
    if (gen != null && (gen.trim().isEmpty || gen == 'Not updated')) {
      gen = null;
    }

    return CustomerModel(
      customerId: json['customerId'] ?? json['CustomerId'] ?? '',
      fullName: json['fullName'] ?? json['FullName'],
      email: json['email'] ?? json['Email'],
      phoneNumber: phone,
      birthDay: rawBirthday,
      gender: gen,
      isVerified: json['isVerified'] ?? json['IsVerified'],
      isActive: json['isActive'] ?? json['IsActive'],
      createdAt: json['createdAt'] != null 
          ? DateTime.tryParse(json['createdAt']) 
          : null,
    );
  }

  // Chuyển đối tượng thành JSON để gửi ngược lại API cập nhật
  Map<String, dynamic> toJson() {
    return {
      'fullName': fullName,
      'phoneNumber': phoneNumber,
      'birthDay': birthDay,
      'gender': gender,
    };
  }
}
