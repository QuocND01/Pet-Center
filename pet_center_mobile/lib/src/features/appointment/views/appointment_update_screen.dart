import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../../constants/app_colors.dart';
import '../../../models/appointment_model.dart';
import '../../../models/booking_page_model.dart';
import '../../../models/available_slot_model.dart';
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

  Future<void> _loadMasterData() async {
    try {
      final data = await _apiService.getBookingData();
      setState(() {
        _masterData = data;
        _isLoading = false;

        // Auto-select matching Pet
        _selectedPet = data.pets.firstWhere(
          (p) => p.petId == widget.detail.petId || p.petName == widget.detail.petName,
          orElse: () => data.pets.first,
        );

        // Auto-select matching Staff
        _selectedStaff = data.staffs.firstWhere(
          (s) => s.staffId == widget.detail.staffId || s.fullName == widget.detail.vetName,
          orElse: () => data.staffs.first,
        );

        // Auto-select matching Services
        _selectedServices = data.services.where(
          (s) => widget.detail.appointmentServices.any((aps) => aps.serviceName == s.serviceName),
        ).toList();
      });

      _loadAvailableSlots();
    } catch (e) {
      setState(() {
        _isLoading = false;
      });
      _showError('Failed to load booking data: $e');
    }
  }

  Future<void> _loadAvailableSlots() async {
    if (_selectedStaff == null || _selectedServices.isEmpty) return;

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

        // Auto-match slot if start time matches
        _selectedSlot = slots.firstWhere(
          (slot) => slot.startTime.hour == widget.detail.appointmentStart.hour &&
              slot.startTime.minute == widget.detail.appointmentStart.minute,
          orElse: () => slots.first,
        );
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
          const SnackBar(content: Text('Appointment updated successfully!'), backgroundColor: Colors.green),
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
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Update Appointment'),
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : SingleChildScrollView(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // 1. Pet Selection
                  const Text('Select Pet', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                  const SizedBox(height: 8),
                  DropdownButtonFormField<BookingPetModel>(
                    value: _selectedPet,
                    decoration: InputDecoration(
                      border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                      filled: true,
                      fillColor: Colors.white,
                    ),
                    items: _masterData?.pets.map((p) {
                          return DropdownMenuItem(value: p, child: Text(p.petName));
                        }).toList() ??
                        [],
                    onChanged: (val) {
                      setState(() {
                        _selectedPet = val;
                      });
                    },
                  ),
                  const SizedBox(height: 16),

                  // 2. Doctor/Staff Selection
                  const Text('Select Doctor / Vet', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                  const SizedBox(height: 8),
                  DropdownButtonFormField<BookingStaffModel>(
                    value: _selectedStaff,
                    decoration: InputDecoration(
                      border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                      filled: true,
                      fillColor: Colors.white,
                    ),
                    items: _masterData?.staffs.map((s) {
                          return DropdownMenuItem(value: s, child: Text(s.fullName));
                        }).toList() ??
                        [],
                    onChanged: (val) {
                      setState(() {
                        _selectedStaff = val;
                      });
                      _loadAvailableSlots();
                    },
                  ),
                  const SizedBox(height: 16),

                  // 3. Services Selection
                  const Text('Select Services', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                  const SizedBox(height: 8),
                  Wrap(
                    spacing: 8,
                    runSpacing: 8,
                    children: _masterData?.services.map((svc) {
                          final isSelected = _selectedServices.any((s) => s.serviceId == svc.serviceId);
                          return FilterChip(
                            selected: isSelected,
                            label: Text('${svc.serviceName} (${svc.price.toStringAsFixed(0)}đ)'),
                            selectedColor: AppColors.primary.withAlpha(50),
                            checkmarkColor: AppColors.primary,
                            onSelected: (selected) {
                              setState(() {
                                if (selected) {
                                  if (_selectedServices.length < 2) {
                                    _selectedServices.add(svc);
                                  } else {
                                    _showError('Maximum 2 services per appointment.');
                                  }
                                } else {
                                  _selectedServices.removeWhere((s) => s.serviceId == svc.serviceId);
                                }
                              });
                              _loadAvailableSlots();
                            },
                          );
                        }).toList() ??
                        [],
                  ),
                  const SizedBox(height: 16),

                  // 4. Date Picker
                  const Text('Select Date', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                  const SizedBox(height: 8),
                  Card(
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                    child: ListTile(
                      leading: const Icon(Icons.calendar_month, color: AppColors.primary),
                      title: Text(DateFormat('EEEE, dd/MM/yyyy').format(_selectedDate)),
                      trailing: const Icon(Icons.edit),
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

                  // 5. Time Slots
                  const Text('Select Time Slot', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                  const SizedBox(height: 8),
                  if (_isLoadingSlots)
                    const Center(child: CircularProgressIndicator())
                  else if (_availableSlots.isEmpty)
                    const Text('No slots available for selected date/doctor.', style: TextStyle(color: Colors.red))
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
                          selectedColor: AppColors.primary,
                          labelStyle: TextStyle(color: isSelected ? Colors.white : Colors.black),
                          onSelected: (val) {
                            setState(() {
                              _selectedSlot = val ? slot : null;
                            });
                          },
                        );
                      }).toList(),
                    ),
                  const SizedBox(height: 16),

                  // 6. Note Field
                  const Text('Note for Doctor', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
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

                  // Submit Button
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
