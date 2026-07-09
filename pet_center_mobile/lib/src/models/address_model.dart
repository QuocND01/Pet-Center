class AddressModel {
  final String addressId;
  final String? province;
  final String? district;
  final String? ward;
  final String? addressDetails;
  final bool isDefault;

  AddressModel({
    required this.addressId,
    this.province,
    this.district,
    this.ward,
    this.addressDetails,
    required this.isDefault,
  });

  factory AddressModel.fromJson(Map<String, dynamic> json) {
    return AddressModel(
      addressId: json['addressId'] ?? json['AddressId'] ?? '',
      province: json['province'] ?? json['Province'],
      district: json['district'] ?? json['District'],
      ward: json['ward'] ?? json['Ward'],
      addressDetails: json['addressDetails'] ?? json['AddressDetails'],
      isDefault: json['isDefault'] ?? json['IsDefault'] ?? false,
    );
  }

  String get fullAddress {
    final parts = [addressDetails, ward, district, province]
        .where((s) => s != null && s.isNotEmpty)
        .toList();
    return parts.isEmpty ? 'Chưa cập nhật địa chỉ' : parts.join(', ');
  }
}
