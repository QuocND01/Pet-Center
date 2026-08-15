import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../../constants/app_colors.dart';
import '../../../models/appointment_model.dart';
import '../../../models/booking_page_model.dart';
import '../../../models/available_slot_model.dart';
import '../../../models/book_appointment_request.dart';
import '../../../services/api_service.dart';
import '../../../widgets/custom_button.dart';

class AppointmentUpdateScreen extends StatefulWidget {
  final AppointmentDetailModel detail;

  const AppointmentUpdateScreen({
    super.key,
    required this.detail,
  });

  @override
  State<AppointmentUpdateScreen> createState() => _AppointmentUpdateScreenState();
}

class _AppointmentUpdateScreenState extends State<AppointmentUpdateScreen> {
  final ApiService _apiService = ApiService();

  bool _isLoading = true;
  bool _isSaving = false;
  BookingPageModel? _masterData;

  BookingPetModel? _selectedPet;
  BookingStaffModel? _selectedStaff;
  List<BookingServiceModel> _selectedServices = [];
  DateTime _selectedDate = DateTime.now();
  AvailableSlotModel? _selectedSlot;
  final TextEditingController _noteCtrl = TextEditingController();

  List<AvailableSlotModel> _availableSlots = [];
  bool _isLoadingSlots = false;

  @override
  void initState() {
    super.initState();
    _noteCtrl.text = widget.detail.note ?? '';
    _selectedDate = widget.detail.appointmentStart;
    _loadMasterData();
  }

  @override
  void dispose() {
    _noteCtrl.dispose();
    super.dispose();
  }

  Future<void> _loadMasterData() async {
    try {
      final data = await _apiService.getBookingData();
      setState(() {
        _masterData = data;
        _isLoading = false;

        // Cố định Pet từ appointment detail
        if (data.pets.isNotEmpty) {
          _selectedPet = data.pets.firstWhere(
            (p) => p.petId == widget.detail.petId || p.petName == widget.detail.petName,
            orElse: () => data.pets.first,
          );
        }

        // Chọn Staff ban đầu
        if (data.staffs.isNotEmpty) {
          _selectedStaff = data.staffs.firstWhere(
            (s) => s.staffId == widget.detail.staffId || s.fullName == widget.detail.vetName,
            orElse: () => data.staffs.first,
          );
        }

        // Khớp các dịch vụ ban đầu tương thích với Specialist
        if (data.services.isNotEmpty) {
          final matchedServices = data.services.where(
            (s) => widget.detail.appointmentServices.any((aps) => aps.serviceName == s.serviceName),
          ).toList();

          _selectedServices = matchedServices.where((s) => _isServiceCompatibleWithStaff(s)).toList();
        }
      });

      _loadAvailableSlots();
    } catch (e) {
      setState(() {
        _isLoading = false;
      });
      _showError('Failed to load booking data: $e');
    }
  }

  bool _isServiceCompatibleWithStaff(BookingServiceModel service) {
    if (_selectedStaff == null) return true;
    final role = (_selectedStaff!.role).trim().toLowerCase();
    if (role == 'vet') {
      return service.serviceType == 1; // Medical
    } else if (role == 'groomer') {
      return service.serviceType == 2; // Grooming
    }
    return true;
  }

  Future<void> _loadAvailableSlots() async {
    if (_selectedStaff == null || _selectedServices.isEmpty) {
      setState(() {
        _availableSlots = [];
        _selectedSlot = null;
      });
      return;
    }

    setState(() {
      _isLoadingSlots = true;
      _availableSlots = [];
      _selectedSlot = null;
    });

    try {
      final dateStr = DateFormat('yyyy-MM-dd').format(_selectedDate);
      final serviceIds = _selectedServices.map((s) => s.serviceId).toList();

      final slots = await _apiService.getAvailableSlots(
        staffId: _selectedStaff!.staffId,
        dateStr: dateStr,
        serviceIds: serviceIds,
      );

      setState(() {
        _availableSlots = slots;
        _isLoadingSlots = false;

        // Khớp lại slot cũ nếu cùng ngày và giờ bắt đầu
        try {
          _selectedSlot = slots.firstWhere(
            (slot) =>
                slot.startTime.year == _selectedDate.year &&
                slot.startTime.month == _selectedDate.month &&
                slot.startTime.day == _selectedDate.day &&
                slot.startTime.hour == widget.detail.appointmentStart.hour &&
                slot.startTime.minute == widget.detail.appointmentStart.minute,
          );
        } catch (_) {
          _selectedSlot = null;
        }
      });
    } catch (_) {
      setState(() {
        _isLoadingSlots = false;
      });
    }
  }

  void _handleSaveUpdate() async {
    if (_selectedSlot == null) {
      _showError('Please select an available time slot.');
      return;
    }
    if (_selectedServices.isEmpty) {
      _showError('Please select at least 1 service.');
      return;
    }

    setState(() {
      _isSaving = true;
    });

    try {
      final req = UpdateAppointmentRequest(
        appointmentId: widget.detail.appointmentId,
        petId: _selectedPet?.petId,
        staffId: _selectedStaff?.staffId,
        serviceIds: _selectedServices.map((s) => s.serviceId).toList(),
        appointmentStart: _selectedSlot!.startTime,
        note: _noteCtrl.text.trim().isNotEmpty ? _noteCtrl.text.trim() : null,
      );

      final ok = await _apiService.updateReservedAppointment(req);
      if (ok) {
        if (!mounted) return;
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Appointment updated successfully!'),
            backgroundColor: Colors.green,
          ),
        );
        Navigator.pop(context, true);
      } else {
        _showError('Failed to update appointment.');
      }
    } catch (e) {
      _showError('Update failed: $e');
    } finally {
      if (mounted) {
        setState(() {
          _isSaving = false;
        });
      }
    }
  }

  void _showError(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message), backgroundColor: AppColors.error),
    );
  }

  @override
  Widget build(BuildContext context) {
    final staffRole = _selectedStaff?.role ?? '';
    final isVet = staffRole.toLowerCase() == 'vet';

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Update Appointment', style: TextStyle(fontWeight: FontWeight.bold)),
        backgroundColor: const Color(0xFF00B4D8),
        foregroundColor: Colors.white,
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator(color: Color(0xFF00B4D8)))
          : SingleChildScrollView(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // 1. PET PATIENT (KHÓA CỐ ĐỊNH)
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      const Text(
                        'Pet Patient',
                        style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
                      ),
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                        decoration: BoxDecoration(
                          color: Colors.grey.shade200,
                          borderRadius: BorderRadius.circular(6),
                        ),
                        child: const Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            Icon(Icons.lock, size: 12, color: Colors.grey),
                            SizedBox(width: 4),
                            Text('LOCKED', style: TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: Colors.grey)),
                          ],
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                  Card(
                    elevation: 0,
                    color: Colors.grey.shade100,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
                      side: BorderSide(color: Colors.grey.shade300),
                    ),
                    child: ListTile(
                      leading: CircleAvatar(
                        backgroundColor: const Color(0xFF00B4D8).withAlpha(30),
                        backgroundImage: (_selectedPet?.petAvatar != null && _selectedPet!.petAvatar!.isNotEmpty)
                            ? NetworkImage(_selectedPet!.petAvatar!)
                            : null,
                        child: (_selectedPet?.petAvatar == null || _selectedPet!.petAvatar!.isEmpty)
                            ? const Icon(Icons.pets, color: Color(0xFF00B4D8))
                            : null,
                      ),
                      title: Text(
                        _selectedPet?.petName ?? widget.detail.petName,
                        style: const TextStyle(fontWeight: FontWeight.bold),
                      ),
                      subtitle: Text(
                        '${_selectedPet?.species ?? "Pet"} • Cannot be changed for this reservation',
                        style: const TextStyle(fontSize: 12, color: Colors.grey),
                      ),
                    ),
                  ),
                  const SizedBox(height: 16),

                  // 2. CHOOSE SPECIALIST (CHO PHÉP CHỌN LẠI)
                  const Text(
                    'Specialist / Doctor',
                    style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
                  ),
                  const SizedBox(height: 8),
                  DropdownButtonFormField<BookingStaffModel>(
                    value: _selectedStaff,
                    isExpanded: true,
                    decoration: InputDecoration(
                      border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                      filled: true,
                      fillColor: Colors.white,
                    ),
                    items: _masterData?.staffs.map((s) {
                          final isVetStaff = s.role.toLowerCase() == 'vet';
                          final displayTitle = isVetStaff ? 'Dr. ${s.fullName}' : s.fullName;
                          return DropdownMenuItem(
                            value: s,
                            child: Row(
                              children: [
                                Expanded(child: Text(displayTitle, overflow: TextOverflow.ellipsis)),
                                Container(
                                  padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                                  decoration: BoxDecoration(
                                    color: isVetStaff ? Colors.teal.shade50 : Colors.amber.shade50,
                                    borderRadius: BorderRadius.circular(6),
                                  ),
                                  child: Text(
                                    s.role.isNotEmpty ? s.role : (isVetStaff ? 'Vet' : 'Groomer'),
                                    style: TextStyle(
                                      fontSize: 11,
                                      fontWeight: FontWeight.bold,
                                      color: isVetStaff ? Colors.teal : Colors.amber.shade900,
                                    ),
                                  ),
                                ),
                              ],
                            ),
                          );
                        }).toList() ??
                        [],
                    onChanged: (val) {
                      if (val != null) {
                        setState(() {
                          _selectedStaff = val;
                          // Tự động bỏ chọn các dịch vụ không tương thích với chuyên môn của staff mới
                          _selectedServices.removeWhere((s) => !_isServiceCompatibleWithStaff(s));
                        });
                        _loadAvailableSlots();
                      }
                    },
                  ),
                  const SizedBox(height: 16),

                  // 3. SELECT CARE SERVICES
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      const Text(
                        'Select Care Services',
                        style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
                      ),
                      Text(
                        'Max 2 services (${isVet ? "Medical" : "Grooming"})',
                        style: const TextStyle(fontSize: 12, color: Colors.grey),
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                  Wrap(
                    spacing: 8,
                    runSpacing: 8,
                    children: _masterData?.services.map((svc) {
                          final isSelected = _selectedServices.any((s) => s.serviceId == svc.serviceId);
                          final isCompatible = _isServiceCompatibleWithStaff(svc);
                          final isMedical = svc.serviceType == 1;

                          return Opacity(
                            opacity: isCompatible ? 1.0 : 0.4,
                            child: FilterChip(
                              selected: isSelected,
                              label: Text(
                                '${svc.serviceName} (${svc.price.toStringAsFixed(0)}đ)',
                                style: TextStyle(
                                  decoration: isCompatible ? null : TextDecoration.lineThrough,
                                  fontSize: 13,
                                ),
                              ),
                              avatar: Container(
                                padding: const EdgeInsets.all(2),
                                decoration: BoxDecoration(
                                  color: isMedical ? Colors.blue.shade100 : Colors.orange.shade100,
                                  shape: BoxShape.circle,
                                ),
                                child: Icon(
                                  isMedical ? Icons.medical_services : Icons.content_cut,
                                  size: 12,
                                  color: isMedical ? Colors.blue.shade800 : Colors.orange.shade800,
                                ),
                              ),
                              selectedColor: const Color(0xFF00B4D8).withAlpha(50),
                              checkmarkColor: const Color(0xFF00B4D8),
                              onSelected: isCompatible
                                  ? (selected) {
                                      setState(() {
                                        if (selected) {
                                          if (_selectedServices.length >= 2) {
                                            _showError('Maximum 2 services per appointment.');
                                            return;
                                          }
                                          if (_selectedServices.isNotEmpty &&
                                              _selectedServices.first.serviceType != svc.serviceType) {
                                            _showError('You can only select services of the same category.');
                                            return;
                                          }
                                          _selectedServices.add(svc);
                                        } else {
                                          _selectedServices.removeWhere((s) => s.serviceId == svc.serviceId);
                                        }
                                      });
                                      _loadAvailableSlots();
                                    }
                                  : null,
                            ),
                          );
                        }).toList() ??
                        [],
                  ),
                  const SizedBox(height: 16),

                  // 4. DATE PICKER
                  const Text('Appointment Date', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                  const SizedBox(height: 8),
                  Card(
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                    child: ListTile(
                      leading: const Icon(Icons.calendar_month, color: Color(0xFF00B4D8)),
                      title: Text(
                        DateFormat('EEEE, dd/MM/yyyy').format(_selectedDate),
                        style: const TextStyle(fontWeight: FontWeight.bold),
                      ),
                      trailing: const Icon(Icons.edit, color: Color(0xFF00B4D8)),
                      onTap: () async {
                        final picked = await showDatePicker(
                          context: context,
                          initialDate: _selectedDate,
                          firstDate: DateTime.now(),
                          lastDate: DateTime.now().add(const Duration(days: 30)),
                        );
                        if (picked != null) {
                          setState(() {
                            _selectedDate = picked;
                          });
                          _loadAvailableSlots();
                        }
                      },
                    ),
                  ),
                  const SizedBox(height: 16),

                  // 5. TIME SLOTS
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      const Text(
                        'Available Time Slots',
                        style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
                      ),
                      TextButton.icon(
                        icon: const Icon(Icons.refresh, size: 16),
                        label: const Text('Refresh'),
                        onPressed: _loadAvailableSlots,
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                  if (_isLoadingSlots)
                    const Center(
                      child: Padding(
                        padding: EdgeInsets.all(16.0),
                        child: CircularProgressIndicator(color: Color(0xFF00B4D8)),
                      ),
                    )
                  else if (_availableSlots.isEmpty)
                    Container(
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        color: Colors.amber.shade50,
                        borderRadius: BorderRadius.circular(10),
                      ),
                      child: const Row(
                        children: [
                          Icon(Icons.info_outline, color: Colors.orange, size: 20),
                          SizedBox(width: 8),
                          Expanded(
                            child: Text(
                              'No slots available for this specialist and date. Please pick another date.',
                              style: TextStyle(fontSize: 13),
                            ),
                          ),
                        ],
                      ),
                    )
                  else
                    Wrap(
                      spacing: 8,
                      runSpacing: 8,
                      children: _availableSlots.map((slot) {
                        final isSelected = _selectedSlot?.startTime == slot.startTime;
                        final timeStr = DateFormat('HH:mm').format(slot.startTime);
                        return ChoiceChip(
                          label: Text(timeStr),
                          selected: isSelected,
                          selectedColor: const Color(0xFF00B4D8),
                          labelStyle: TextStyle(
                            color: isSelected ? Colors.white : Colors.black87,
                            fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
                          ),
                          onSelected: (val) {
                            setState(() {
                              _selectedSlot = val ? slot : null;
                            });
                          },
                        );
                      }).toList(),
                    ),
                  const SizedBox(height: 16),

                  // 6. NOTE
                  const Text('Note for Specialist', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                  const SizedBox(height: 8),
                  TextFormField(
                    controller: _noteCtrl,
                    maxLines: 2,
                    decoration: InputDecoration(
                      hintText: 'Enter notes or specific requests...',
                      border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                      filled: true,
                      fillColor: Colors.white,
                    ),
                  ),
                  const SizedBox(height: 24),

                  // 7. SUBMIT BUTTON
                  CustomButton(
                    text: 'Save Changes',
                    isLoading: _isSaving,
                    onPressed: _handleSaveUpdate,
                  ),
                ],
              ),
            ),
    );
  }
}