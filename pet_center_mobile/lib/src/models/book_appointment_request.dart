class BookAppointmentRequest {
  final String customerId;
  final String petId;
  final String staffId;
  final DateTime appointmentStart;
  final String? note;
  final List<String> serviceIds;

  BookAppointmentRequest({
    required this.customerId,
    required this.petId,
    required this.staffId,
    required this.appointmentStart,
    this.note,
    required this.serviceIds,
  });

  Map<String, dynamic> toJson() {
    return {
      'customerId': customerId,
      'petId': petId,
      'staffId': staffId,
      'appointmentStart': appointmentStart.toIso8601String(),
      'note': note,
      'serviceIds': serviceIds,
    };
  }
}