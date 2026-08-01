class PrescriptionItemModel {
  final String prescriptionItemId;
  final String recordId;
  final String medicineName;
  final String dosage;
  final String duration;
  final int quantity;
  final String? note;

  PrescriptionItemModel({
    required this.prescriptionItemId,
    required this.recordId,
    required this.medicineName,
    required this.dosage,
    required this.duration,
    required this.quantity,
    this.note,
  });

  factory PrescriptionItemModel.fromJson(Map<String, dynamic> json) {
    return PrescriptionItemModel(
      prescriptionItemId: json['prescriptionItemId'] ?? json['PrescriptionItemId'] ?? '',
      recordId: json['recordId'] ?? json['RecordId'] ?? '',
      medicineName: json['medicineName'] ?? json['MedicineName'] ?? '',
      dosage: json['dosage'] ?? json['Dosage'] ?? '',
      duration: json['duration'] ?? json['Duration'] ?? '',
      quantity: json['quantity'] ?? json['Quantity'] ?? 1,
      note: json['note'] ?? json['Note'],
    );
  }
}

class MedicalRecordModel {
  final String recordId;
  final String? appointmentId;
  final String customerId;
  final String? diseaseId;
  final String? diseaseNameSnapshot;
  final String diagnosis;
  final String treatment;
  final String? note;
  final DateTime? createdAt;
  final int? status;
  final String statusName;
  final DateTime? appointmentStart;
  final String customerName;
  final String petSpecies;
  final String petBreed;
  final String vetName;
  final List<PrescriptionItemModel> prescriptionItems;

  MedicalRecordModel({
    required this.recordId,
    this.appointmentId,
    required this.customerId,
    this.diseaseId,
    this.diseaseNameSnapshot,
    required this.diagnosis,
    required this.treatment,
    this.note,
    this.createdAt,
    this.status,
    required this.statusName,
    this.appointmentStart,
    required this.customerName,
    required this.petSpecies,
    required this.petBreed,
    required this.vetName,
    required this.prescriptionItems,
  });

  factory MedicalRecordModel.fromJson(Map<String, dynamic> json) {
    DateTime? parseDate(dynamic val) {
      if (val == null) return null;
      try {
        return DateTime.parse(val.toString());
      } catch (_) {
        return null;
      }
    }

    var rawRx = json['prescriptionItems'] ?? json['PrescriptionItems'];
    List<PrescriptionItemModel> rxList = [];
    if (rawRx is List) {
      rxList = rawRx.map((item) => PrescriptionItemModel.fromJson(item)).toList();
    }

    return MedicalRecordModel(
      recordId: json['recordId'] ?? json['RecordId'] ?? '',
      appointmentId: json['appointmentId'] ?? json['AppointmentId'],
      customerId: json['customerId'] ?? json['CustomerId'] ?? '',
      diseaseId: json['diseaseId'] ?? json['DiseaseId'],
      diseaseNameSnapshot: json['diseaseNameSnapshot'] ?? json['DiseaseNameSnapshot'],
      diagnosis: json['diagnosis'] ?? json['Diagnosis'] ?? 'N/A',
      treatment: json['treatment'] ?? json['Treatment'] ?? 'N/A',
      note: json['note'] ?? json['Note'],
      createdAt: parseDate(json['createdAt'] ?? json['CreatedAt']),
      status: json['status'] ?? json['Status'],
      statusName: json['statusName'] ?? json['StatusName'] ?? 'Completed',
      appointmentStart: parseDate(json['appointmentStart'] ?? json['AppointmentStart']),
      customerName: json['customerName'] ?? json['CustomerName'] ?? '',
      petSpecies: json['petSpecies'] ?? json['PetSpecies'] ?? 'Pet',
      petBreed: json['petBreed'] ?? json['PetBreed'] ?? '',
      vetName: json['vetName'] ?? json['VetName'] ?? 'Veterinarian',
      prescriptionItems: rxList,
    );
  }
}
