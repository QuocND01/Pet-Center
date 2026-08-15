import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../../services/api_service.dart';
import '../../../models/booking_page_model.dart';
import '../../../models/available_slot_model.dart';
import '../../../models/book_appointment_request.dart';
import 'appointment_detail_screen.dart';

class BookingScreen extends StatefulWidget {
  final String? initialServiceId;
  final String? initialPetId;

  const BookingScreen({
    Key? key,
    this.initialServiceId,
    this.initialPetId,
  }) : super(key: key);

  @override
  State<BookingScreen> createState() => _BookingScreenState();
}

class _BookingScreenState extends State<BookingScreen> {
  final ApiService _apiService = ApiService();

  // Initial Data Loading State
  bool _isLoading = true;
  String? _errorMessage;
  BookingPageModel? _bookingData;

  // Stepper State (1 -> 4)
  int _currentStep = 1;

  // Form State Selection
  BookingPetModel? _selectedPet;
  BookingStaffModel? _selectedStaff;
  List<BookingServiceModel> _selectedServices = [];

  DateTime _selectedDate = DateTime.now();
  AvailableSlotModel? _selectedSlot;
  final TextEditingController _noteController = TextEditingController();

  // Time Slots API State
  bool _isLoadingSlots = false;
  List<AvailableSlotModel> _availableSlots = [];
  bool _hasSearchedSlots = false;

  @override
  void initState() {
    super.initState();
    _fetchBookingPageData();
  }

  @override
  void dispose() {
    _noteController.dispose();
    super.dispose();
  }

  // Fetch initial data from API
  Future<void> _fetchBookingPageData() async {
    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    try {
      final data = await _apiService.getBookingData();

      BookingPetModel? preSelectedPet;
      if (widget.initialPetId != null && data.pets.isNotEmpty) {
        try {
          preSelectedPet = data.pets.firstWhere((p) => p.petId == widget.initialPetId);
        } catch (_) {}
      }

      List<BookingServiceModel> preSelectedServices = [];
      if (widget.initialServiceId != null && data.services.isNotEmpty) {
        try {
          final s = data.services.firstWhere((srv) => srv.serviceId == widget.initialServiceId);
          preSelectedServices.add(s);
        } catch (_) {}
      }

      setState(() {
        _bookingData = data;
        if (preSelectedPet != null) {
          _selectedPet = preSelectedPet;
        }
        if (preSelectedServices.isNotEmpty) {
          _selectedServices = preSelectedServices;
        }
        _isLoading = false;
      });
    } catch (e) {
      setState(() {
        _errorMessage = e.toString().replaceAll('Exception: ', '');
        _isLoading = false;
      });
    }
  }

  // Kiểm tra Service có tương thích với Role của Staff đang chọn hay không
  bool _isServiceCompatibleWithStaff(BookingServiceModel service) {
    if (_selectedStaff == null) return true;
    final role = (_selectedStaff!.role ?? '').trim().toLowerCase();
    if (role == 'vet') {
      return service.serviceType == 1; // Y tế
    } else if (role == 'groomer') {
      return service.serviceType == 2; // Grooming
    }
    return true;
  }

  void _resetSlots() {
    _selectedSlot = null;
    _availableSlots = [];
    _hasSearchedSlots = false;
  }

  // Call API for Available Time Slots at Step 4
  Future<void> _loadAvailableSlots() async {
    if (_selectedStaff == null) {
      _showToast('Please go back and select a specialist.', isError: true);
      return;
    }
    if (_selectedServices.isEmpty) {
      _showToast('Please go back and select care services.', isError: true);
      return;
    }

    setState(() {
      _isLoadingSlots = true;
      _availableSlots = [];
      _selectedSlot = null;
      _hasSearchedSlots = true;
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
      });
    } catch (e) {
      setState(() {
        _isLoadingSlots = false;
      });
      _showToast(e.toString().replaceAll('Exception: ', ''), isError: true);
    }
  }

  // Handle Form Submission
  Future<void> _submitBooking() async {
    if (_selectedSlot == null) {
      _showToast('Please select a time slot for your appointment!', isError: true);
      return;
    }

    final customerId = _apiService.customerId;
    if (customerId == null || customerId.isEmpty) {
      _showToast('Customer account not found. Please log in again.', isError: true);
      return;
    }

    // Show loading dialog
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (_) => const Center(child: CircularProgressIndicator()),
    );

    try {
      final req = BookAppointmentRequest(
        customerId: customerId,
        petId: _selectedPet!.petId,
        staffId: _selectedStaff!.staffId,
        appointmentStart: _selectedSlot!.startTime,
        note: _noteController.text.trim().isNotEmpty ? _noteController.text.trim() : null,
        serviceIds: _selectedServices.map((s) => s.serviceId).toList(),
      );

      final result = await _apiService.bookAppointment(req);

      if (mounted) Navigator.pop(context); // Close Loading Dialog

      _showToast('Book appointment successfully!', isError: false);

      String? appointmentId;
      if (result is Map) {
        appointmentId = (result['appointmentId'] ?? result['AppointmentId'])?.toString();
      }

      if (mounted) {
        if (appointmentId != null && appointmentId.isNotEmpty) {
          Navigator.pushReplacement(
            context,
            MaterialPageRoute(
              builder: (context) => AppointmentDetailScreen(appointmentId: appointmentId!),
            ),
          );
        } else {
          Navigator.pop(context, true);
        }
      }
    } catch (e) {
      if (mounted) Navigator.pop(context); // Close Loading Dialog
      _showToast(e.toString().replaceAll('Exception: ', ''), isError: true);
    }
  }

  void _showToast(String message, {bool isError = false}) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(message),
        backgroundColor: isError ? Colors.red : Colors.green,
        duration: const Duration(seconds: 3),
      ),
    );
  }

  // Helper for step progression validation
  bool _canAdvanceFromCurrentStep() {
    switch (_currentStep) {
      case 1:
        return _selectedPet != null;
      case 2:
        return _selectedStaff != null;
      case 3:
        return _selectedServices.isNotEmpty;
      case 4:
        return _selectedSlot != null;
      default:
        return false;
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF8F9FA),
      appBar: AppBar(
        title: const Text('Book Appointment', style: TextStyle(fontWeight: FontWeight.bold)),
        backgroundColor: const Color(0xFF00B4D8),
        foregroundColor: Colors.white,
        elevation: 0,
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator(color: Color(0xFF00B4D8)))
          : _errorMessage != null
              ? Center(
                  child: Padding(
                    padding: const EdgeInsets.all(24.0),
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        const Icon(Icons.error_outline, size: 64, color: Colors.red),
                        const SizedBox(height: 16),
                        Text(_errorMessage!, textAlign: TextAlign.center, style: const TextStyle(fontSize: 16)),
                        const SizedBox(height: 16),
                        ElevatedButton(
                          onPressed: _fetchBookingPageData,
                          style: ElevatedButton.styleFrom(backgroundColor: const Color(0xFF00B4D8)),
                          child: const Text('Retry', style: TextStyle(color: Colors.white)),
                        ),
                      ],
                    ),
                  ),
                )
              : Column(
                  children: [
                    // Stepper Indicator
                    _buildStepperHeader(),

                    // Step Content
                    Expanded(
                      child: SingleChildScrollView(
                        padding: const EdgeInsets.all(16.0),
                        child: _buildCurrentStepContent(),
                      ),
                    ),

                    // Bottom Navigation Bar
                    _buildBottomNavigation(),
                  ],
                ),
    );
  }

  // Stepper Header (1 -> 4)
  Widget _buildStepperHeader() {
    return Container(
      color: Colors.white,
      padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 8),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceEvenly,
        children: [
          _buildStepHeaderItem(1, 'Pet', Icons.pets),
          _buildStepLine(1),
          _buildStepHeaderItem(2, 'Specialist', Icons.medical_services),
          _buildStepLine(2),
          _buildStepHeaderItem(3, 'Service', Icons.local_hospital),
          _buildStepLine(3),
          _buildStepHeaderItem(4, 'Schedule', Icons.calendar_month),
        ],
      ),
    );
  }

  Widget _buildStepHeaderItem(int stepNumber, String title, IconData icon) {
    final isActive = _currentStep == stepNumber;
    final isDone = _currentStep > stepNumber;

    return InkWell(
      onTap: () {
        if (stepNumber < _currentStep) {
          setState(() {
            _currentStep = stepNumber;
          });
        }
      },
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          CircleAvatar(
            radius: 18,
            backgroundColor: isDone
                ? const Color(0xFF00B4D8)
                : isActive
                    ? const Color(0xFF00B4D8)
                    : Colors.grey.shade300,
            child: Icon(
              isDone ? Icons.check : icon,
              size: 18,
              color: (isActive || isDone) ? Colors.white : Colors.grey.shade600,
            ),
          ),
          const SizedBox(height: 4),
          Text(
            title,
            style: TextStyle(
              fontSize: 12,
              fontWeight: isActive ? FontWeight.bold : FontWeight.normal,
              color: isActive ? const Color(0xFF00B4D8) : Colors.grey.shade600,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildStepLine(int afterStep) {
    final isDone = _currentStep > afterStep;
    return Expanded(
      child: Container(
        height: 2,
        color: isDone ? const Color(0xFF00B4D8) : Colors.grey.shade300,
      ),
    );
  }

  Widget _buildCurrentStepContent() {
    switch (_currentStep) {
      case 1:
        return _buildStep1PetSelection();
      case 2:
        return _buildStep2StaffSelection();
      case 3:
        return _buildStep3ServiceSelection();
      case 4:
        return _buildStep4ScheduleSelection();
      default:
        return Container();
    }
  }

  // STEP 1: Select Pet
  Widget _buildStep1PetSelection() {
    final pets = _bookingData?.pets ?? [];

    if (pets.isEmpty) {
      return Center(
        child: Column(
          children: [
            const Icon(Icons.pets, size: 64, color: Colors.grey),
            const SizedBox(height: 16),
            const Text('You have no registered pets yet.', style: TextStyle(fontSize: 16)),
            const SizedBox(height: 12),
            ElevatedButton(
              onPressed: () {
                Navigator.pushNamed(context, '/pets');
              },
              child: const Text('Add Pet Now'),
            ),
          ],
        ),
      );
    }

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text('Select Pet Patient', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
        const SizedBox(height: 4),
        const Text('Choose the pet needing examination or care:', style: TextStyle(color: Colors.grey)),
        const SizedBox(height: 16),
        ListView.builder(
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          itemCount: pets.length,
          itemBuilder: (context, index) {
            final pet = pets[index];
            final isSelected = _selectedPet?.petId == pet.petId;

            return Card(
              margin: const EdgeInsets.only(bottom: 12),
              elevation: isSelected ? 3 : 1,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(12),
                side: BorderSide(
                  color: isSelected ? const Color(0xFF00B4D8) : Colors.transparent,
                  width: 2,
                ),
              ),
              child: ListTile(
                leading: CircleAvatar(
                  backgroundColor: const Color(0xFF00B4D8).withAlpha(30),
                  backgroundImage: (pet.petAvatar != null && pet.petAvatar!.isNotEmpty)
                      ? NetworkImage(pet.petAvatar!)
                      : null,
                  child: (pet.petAvatar == null || pet.petAvatar!.isEmpty)
                      ? const Icon(Icons.pets, color: Color(0xFF00B4D8))
                      : null,
                ),
                title: Text(pet.petName, style: const TextStyle(fontWeight: FontWeight.bold)),
                subtitle: Text('${pet.species ?? "Pet"}${pet.breed != null ? " • ${pet.breed}" : ""}'),
                trailing: isSelected
                    ? const Icon(Icons.check_circle, color: Color(0xFF00B4D8))
                    : const Icon(Icons.circle_outlined, color: Colors.grey),
                onTap: () {
                  setState(() {
                    _selectedPet = pet;
                  });
                },
              ),
            );
          },
        ),
      ],
    );
  }

  // STEP 2: Select Specialist (Vet / Groomer)
  Widget _buildStep2StaffSelection() {
    final staffs = _bookingData?.staffs ?? [];

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text('Choose Specialist', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
        const SizedBox(height: 4),
        const Text('Select a veterinarian or pet groomer:', style: TextStyle(color: Colors.grey)),
        const SizedBox(height: 16),
        ListView.builder(
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          itemCount: staffs.length,
          itemBuilder: (context, index) {
            final staff = staffs[index];
            final isSelected = _selectedStaff?.staffId == staff.staffId;
            final isVet = (staff.role ?? '').toLowerCase() == 'vet';
            final displayTitle = isVet ? 'Dr. ${staff.fullName}' : staff.fullName;

            return Card(
              margin: const EdgeInsets.only(bottom: 12),
              elevation: isSelected ? 3 : 1,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(12),
                side: BorderSide(
                  color: isSelected ? const Color(0xFF00B4D8) : Colors.transparent,
                  width: 2,
                ),
              ),
              child: ListTile(
                leading: CircleAvatar(
                  backgroundColor: isVet ? Colors.teal.shade50 : Colors.amber.shade50,
                  backgroundImage: (staff.avatar != null && staff.avatar!.isNotEmpty)
                      ? NetworkImage(staff.avatar!)
                      : null,
                  child: (staff.avatar == null || staff.avatar!.isEmpty)
                      ? Icon(isVet ? Icons.medical_services : Icons.content_cut,
                          color: isVet ? Colors.teal : Colors.amber.shade800)
                      : null,
                ),
                title: Row(
                  children: [
                    Expanded(
                      child: Text(displayTitle, style: const TextStyle(fontWeight: FontWeight.bold)),
                    ),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                      decoration: BoxDecoration(
                        color: isVet ? Colors.teal.shade50 : Colors.amber.shade50,
                        borderRadius: BorderRadius.circular(8),
                      ),
                      child: Text(
                        staff.role ?? (isVet ? 'Vet' : 'Groomer'),
                        style: TextStyle(
                          fontSize: 11,
                          fontWeight: FontWeight.bold,
                          color: isVet ? Colors.teal : Colors.amber.shade900,
                        ),
                      ),
                    ),
                  ],
                ),
                subtitle: Text(
                  '${staff.experienceYears != null ? "${staff.experienceYears!.toStringAsFixed(0)} Yrs Exp" : "Specialist"}${staff.phoneNumber.isNotEmpty ? " • ${staff.phoneNumber}" : ""}',
                ),
                trailing: isSelected
                    ? const Icon(Icons.check_circle, color: Color(0xFF00B4D8))
                    : const Icon(Icons.circle_outlined, color: Colors.grey),
                onTap: () {
                  setState(() {
                    _selectedStaff = staff;
                    // Lọc bỏ những dịch vụ không tương thích với chuyên môn của staff mới chọn
                    _selectedServices.removeWhere((s) => !_isServiceCompatibleWithStaff(s));
                    _resetSlots();
                  });
                },
              ),
            );
          },
        ),
      ],
    );
  }

  // STEP 3: Select Services (Có filter khóa theo Role của Staff)
  Widget _buildStep3ServiceSelection() {
    final services = _bookingData?.services ?? [];
    final staffRole = _selectedStaff?.role ?? '';
    final isVet = staffRole.toLowerCase() == 'vet';

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text('Select Care Services', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
        const SizedBox(height: 4),
        Text(
          _selectedStaff != null
              ? 'Showing services for ${_selectedStaff!.fullName} (${isVet ? "Medical Services only" : "Grooming Services only"}):'
              : 'You can select up to 2 services of the same type:',
          style: const TextStyle(color: Colors.grey),
        ),
        const SizedBox(height: 16),
        ListView.builder(
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          itemCount: services.length,
          itemBuilder: (context, index) {
            final service = services[index];
            final isSelected = _selectedServices.any((s) => s.serviceId == service.serviceId);
            final isCompatible = _isServiceCompatibleWithStaff(service);
            final isMedical = service.serviceType == 1;

            return Opacity(
              opacity: isCompatible ? 1.0 : 0.4,
              child: Card(
                margin: const EdgeInsets.only(bottom: 12),
                elevation: isSelected ? 3 : 1,
                color: isCompatible ? Colors.white : Colors.grey.shade100,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12),
                  side: BorderSide(
                    color: isSelected ? const Color(0xFF00B4D8) : Colors.transparent,
                    width: 2,
                  ),
                ),
                child: CheckboxListTile(
                  value: isSelected,
                  activeColor: const Color(0xFF00B4D8),
                  title: Row(
                    children: [
                      Expanded(
                        child: Text(
                          service.serviceName,
                          style: TextStyle(
                            fontWeight: FontWeight.bold,
                            decoration: isCompatible ? null : TextDecoration.lineThrough,
                          ),
                        ),
                      ),
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                        decoration: BoxDecoration(
                          color: isMedical ? Colors.blue.shade50 : Colors.orange.shade50,
                          borderRadius: BorderRadius.circular(6),
                        ),
                        child: Text(
                          isMedical ? 'Medical' : 'Grooming',
                          style: TextStyle(
                            fontSize: 10,
                            fontWeight: FontWeight.bold,
                            color: isMedical ? Colors.blue.shade700 : Colors.orange.shade800,
                          ),
                        ),
                      ),
                    ],
                  ),
                  subtitle: Text(
                    '${service.price.toStringAsFixed(0)}đ • ${service.duration} mins',
                    style: const TextStyle(color: Color(0xFF00B4D8), fontWeight: FontWeight.w600),
                  ),
                  onChanged: isCompatible
                      ? (checked) {
                          setState(() {
                            if (checked == true) {
                              if (_selectedServices.length >= 2) {
                                _showToast('Maximum 2 services allowed per appointment.', isError: true);
                                return;
                              }
                              // Kiểm tra cùng category nếu đã có 1 dịch vụ
                              if (_selectedServices.isNotEmpty &&
                                  _selectedServices.first.serviceType != service.serviceType) {
                                _showToast('You can only select services of the same category.', isError: true);
                                return;
                              }
                              _selectedServices.add(service);
                            } else {
                              _selectedServices.removeWhere((s) => s.serviceId == service.serviceId);
                            }
                            _resetSlots();
                          });
                        }
                      : null,
                ),
              ),
            );
          },
        ),
      ],
    );
  }

  // STEP 4: Select Schedule & Slots
  Widget _buildStep4ScheduleSelection() {
    final dateFormatted = DateFormat('EEEE, dd/MM/yyyy').format(_selectedDate);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text('Appointment Schedule', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
        const SizedBox(height: 12),

        // Date Picker Button
        Card(
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
          child: ListTile(
            leading: const Icon(Icons.calendar_month, color: Color(0xFF00B4D8)),
            title: Text(dateFormatted, style: const TextStyle(fontWeight: FontWeight.bold)),
            subtitle: const Text('Tap to change appointment date'),
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

        // Time Slots Area
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            const Text('Available Time Slots', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
            TextButton.icon(
              icon: const Icon(Icons.refresh, size: 18),
              label: const Text('Refresh Slots'),
              onPressed: _loadAvailableSlots,
            ),
          ],
        ),
        const SizedBox(height: 8),

        if (!_hasSearchedSlots && !_isLoadingSlots) ...[
          Center(
            child: Padding(
              padding: const EdgeInsets.all(16.0),
              child: ElevatedButton.icon(
                onPressed: _loadAvailableSlots,
                icon: const Icon(Icons.search),
                label: const Text('Find Available Time Slots'),
                style: ElevatedButton.styleFrom(backgroundColor: const Color(0xFF00B4D8), foregroundColor: Colors.white),
              ),
            ),
          ),
        ] else if (_isLoadingSlots) ...[
          const Center(child: Padding(padding: EdgeInsets.all(24.0), child: CircularProgressIndicator(color: Color(0xFF00B4D8)))),
        ] else if (_availableSlots.isEmpty) ...[
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(color: Colors.amber.shade50, borderRadius: BorderRadius.circular(12)),
            child: const Row(
              children: [
                Icon(Icons.info_outline, color: Colors.orange),
                SizedBox(width: 12),
                Expanded(child: Text('No slots available for the selected specialist/date. Please pick another date.')),
              ],
            ),
          ),
        ] else ...[
          Wrap(
            spacing: 10,
            runSpacing: 10,
            children: _availableSlots.map((slot) {
              final isSelected = _selectedSlot?.startTime == slot.startTime;
              final timeStr = DateFormat('HH:mm').format(slot.startTime);

              return ChoiceChip(
                label: Text(timeStr, style: TextStyle(fontWeight: FontWeight.bold, color: isSelected ? Colors.white : Colors.black87)),
                selected: isSelected,
                selectedColor: const Color(0xFF00B4D8),
                backgroundColor: Colors.white,
                onSelected: (selected) {
                  setState(() {
                    _selectedSlot = selected ? slot : null;
                  });
                },
              );
            }).toList(),
          ),
        ],

        const SizedBox(height: 24),
        const Text('Additional Notes', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
        const SizedBox(height: 8),
        TextFormField(
          controller: _noteController,
          maxLines: 2,
          decoration: InputDecoration(
            hintText: 'Enter any symptoms or requests for the specialist...',
            border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
            filled: true,
            fillColor: Colors.white,
          ),
        ),
      ],
    );
  }

  // Bottom Navigation Bar
  Widget _buildBottomNavigation() {
    final isLastStep = _currentStep == 4;

    return Container(
      color: Colors.white,
      padding: const EdgeInsets.all(16),
      child: Row(
        children: [
          if (_currentStep > 1) ...[
            Expanded(
              child: OutlinedButton(
                onPressed: () {
                  setState(() {
                    _currentStep--;
                  });
                },
                style: OutlinedButton.styleFrom(minimumSize: const Size(0, 48)),
                child: const Text('Back'),
              ),
            ),
            const SizedBox(width: 12),
          ],
          Expanded(
            child: ElevatedButton(
              onPressed: _canAdvanceFromCurrentStep()
                  ? () {
                      if (isLastStep) {
                        _submitBooking();
                      } else {
                        setState(() {
                          _currentStep++;
                        });
                        if (_currentStep == 4 && !_hasSearchedSlots) {
                          _loadAvailableSlots();
                        }
                      }
                    }
                  : null,
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xFF00B4D8),
                foregroundColor: Colors.white,
                minimumSize: const Size(0, 48),
              ),
              child: Text(isLastStep ? 'Confirm & Book' : 'Next'),
            ),
          ),
        ],
      ),
    );
  }
}