import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../services/api_service.dart';
import '../../models/booking_page_model.dart';
import '../../models/available_slot_model.dart';
import '../../models/book_appointment_request.dart';

class BookingScreen extends StatefulWidget {
  const BookingScreen({Key? key}) : super(key: key);

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
      setState(() {
        _bookingData = data;
        _isLoading = false;
      });
    } catch (e) {
      setState(() {
        _errorMessage = e.toString().replaceAll('Exception: ', '');
        _isLoading = false;
      });
    }
  }

  // Call API for Available Time Slots at Step 4
  Future<void> _loadAvailableSlots() async {
    if (_selectedStaff == null) {
      _showToast('Please go back and select a doctor.', isError: true);
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

      await _apiService.bookAppointment(req);

      if (mounted) Navigator.pop(context); // Close Loading Dialog

      // Show success toast
      _showToast('Book appointment successfully!', isError: false);

      // Return back
      if (mounted) {
        Navigator.pop(context, true);
      }
    } catch (e) {
      if (mounted) Navigator.pop(context); // Close Loading Dialog
      _showToast(e.toString().replaceAll('Exception: ', ''), isError: true);
    }
  }

  // Toast SnackBar Helper
  void _showToast(String message, {bool isError = false}) {
    ScaffoldMessenger.of(context).hideCurrentSnackBar();
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Row(
          children: [
            Icon(
              isError ? Icons.error_outline : Icons.check_circle_outline,
              color: Colors.white,
            ),
            const SizedBox(width: 10),
            Expanded(
              child: Text(
                message,
                style: const TextStyle(fontWeight: FontWeight.w600),
              ),
            ),
          ],
        ),
        backgroundColor: isError ? Colors.red.shade700 : Colors.green.shade700,
        behavior: SnackBarBehavior.floating,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
        margin: const EdgeInsets.all(12),
      ),
    );
  }

  // Calculate Total Price & Duration
  double get _totalPrice => _selectedServices.fold(0, (sum, s) => sum + s.price);
  int get _totalDuration => _selectedServices.fold(0, (sum, s) => sum + s.duration);

  // Stepper Navigation
  void _nextStep() {
    if (_currentStep == 1 && _selectedPet == null) {
      _showToast('Please select a pet before moving forward.');
      return;
    }
    if (_currentStep == 2 && _selectedStaff == null) {
      _showToast('Please select a doctor before moving forward.');
      return;
    }
    if (_currentStep == 3 && _selectedServices.isEmpty) {
      _showToast('Please select at least one service before moving forward.');
      return;
    }

    if (_currentStep < 4) {
      setState(() {
        _currentStep++;
      });
    }
  }

  void _prevStep() {
    if (_currentStep > 1) {
      setState(() {
        _currentStep--;
      });
    }
  }

  

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Book Appointment', style: TextStyle(fontWeight: FontWeight.bold)),
        centerTitle: true,
        elevation: 0,
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : _errorMessage != null
              ? Center(
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Text(_errorMessage!, style: const TextStyle(color: Colors.red)),
                      const SizedBox(height: 12),
                      ElevatedButton(
                        onPressed: _fetchBookingPageData,
                        child: const Text('Retry'),
                      )
                    ],
                  ),
                )
              : Column(
                  children: [
                    _buildStepperHeader(),
                    Expanded(
                      child: SingleChildScrollView(
                        padding: const EdgeInsets.all(16.0),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            if (_currentStep == 1) _buildStep1Pet(),
                            if (_currentStep == 2) _buildStep2Staff(),
                            if (_currentStep == 3) _buildStep3Services(),
                            if (_currentStep == 4) _buildStep4Slots(),
                            const SizedBox(height: 20),
                            _buildSummaryCard(),
                          ],
                        ),
                      ),
                    ),
                    _buildBottomNavigation(),
                  ],
                ),
    );
  }

  // ==========================================
  // STEPPER HEADER
  // ==========================================
  Widget _buildStepperHeader() {
    final steps = [
      {'icon': Icons.pets, 'label': 'Pet'},
      {'icon': Icons.medical_services, 'label': 'Doctor'},
      {'icon': Icons.cleaning_services, 'label': 'Services'},
      {'icon': Icons.calendar_month, 'label': 'Time Slot'},
    ];

    return Container(
      color: Theme.of(context).cardColor,
      padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 8),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceAround,
        children: List.generate(steps.length, (index) {
          final stepNum = index + 1;
          final isActive = stepNum <= _currentStep;
          final isCurrent = stepNum == _currentStep;

          return Column(
            children: [
              CircleAvatar(
                radius: 18,
                backgroundColor: isActive ? Theme.of(context).primaryColor : Colors.grey.shade300,
                child: Icon(
                  steps[index]['icon'] as IconData,
                  size: 18,
                  color: isActive ? Colors.white : Colors.grey.shade600,
                ),
              ),
              const SizedBox(height: 4),
              Text(
                steps[index]['label'] as String,
                style: TextStyle(
                  fontSize: 12,
                  fontWeight: isCurrent ? FontWeight.bold : FontWeight.normal,
                  color: isActive ? Theme.of(context).primaryColor : Colors.grey,
                ),
              )
            ],
          );
        }),
      ),
    );
  }

  // ==========================================
  // STEP 1: SELECT PET
  // ==========================================
  Widget _buildStep1Pet() {
    final pets = _bookingData?.pets ?? [];

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text('Select Your Pet', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
        const Text('Which furry friend are we taking care of today?', style: TextStyle(color: Colors.grey, fontSize: 13)),
        const SizedBox(height: 16),
        if (pets.isEmpty)
          const Center(child: Text('No pets registered yet.'))
        else
          GridView.builder(
            shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            itemCount: pets.length,
            gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
              crossAxisCount: 2,
              childAspectRatio: 0.85,
              crossAxisSpacing: 12,
              mainAxisSpacing: 12,
            ),
            itemBuilder: (context, index) {
              final pet = pets[index];
              final isSelected = _selectedPet?.petId == pet.petId;

              return GestureDetector(
                onTap: () {
                  setState(() {
                    _selectedPet = pet;
                  });
                },
                child: Container(
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(16),
                    border: Border.all(
                      color: isSelected ? Theme.of(context).primaryColor : Colors.grey.shade300,
                      width: isSelected ? 2.5 : 1,
                    ),
                    boxShadow: [
                      BoxShadow(color: Colors.black.withOpacity(0.04), blurRadius: 6, offset: const Offset(0, 3)),
                    ],
                  ),
                  child: Stack(
                    children: [
                      Padding(
                        padding: const EdgeInsets.all(12.0),
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            CircleAvatar(
                              radius: 36,
                              backgroundImage: pet.petAvatar != null && pet.petAvatar!.isNotEmpty
                                  ? NetworkImage(pet.petAvatar!)
                                  : null,
                              child: pet.petAvatar == null || pet.petAvatar!.isEmpty
                                  ? const Icon(Icons.pets, size: 36)
                                  : null,
                            ),
                            const SizedBox(height: 8),
                            Text(
                              pet.petName,
                              style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15),
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                            Text(
                              '${pet.species ?? ""} ${pet.breed != null ? "• ${pet.breed}" : ""}',
                              style: const TextStyle(color: Colors.grey, fontSize: 12),
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                          ],
                        ),
                      ),
                      Positioned(
                        top: 6,
                        right: 6,
                        child: IconButton(
                          icon: const Icon(Icons.info_outline, size: 20, color: Colors.grey),
                          onPressed: () => _showPetDetailModal(pet),
                        ),
                      ),
                      if (isSelected)
                        Positioned(
                          top: 8,
                          left: 8,
                          child: Icon(Icons.check_circle, color: Theme.of(context).primaryColor, size: 22),
                        ),
                    ],
                  ),
                ),
              );
            },
          ),
      ],
    );
  }

  // ==========================================
  // STEP 2: SELECT DOCTOR
  // ==========================================
  Widget _buildStep2Staff() {
    final staffs = _bookingData?.staffs ?? [];

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text('Choose Veterinary Doctor', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
        const Text('Select a trusted specialist or let us assign the best one.', style: TextStyle(color: Colors.grey, fontSize: 13)),
        const SizedBox(height: 16),
        ListView.separated(
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          itemCount: staffs.length,
          separatorBuilder: (_, __) => const SizedBox(height: 12),
          itemBuilder: (context, index) {
            final staff = staffs[index];
            final isSelected = _selectedStaff?.staffId == staff.staffId;

            return InkWell(
              onTap: () {
                setState(() {
                  _selectedStaff = staff;
                  // Reset slots when staff changes
                  _selectedSlot = null;
                  _availableSlots = [];
                  _hasSearchedSlots = false;
                });
              },
              borderRadius: BorderRadius.circular(16),
              child: Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(16),
                  border: Border.all(
                    color: isSelected ? Theme.of(context).primaryColor : Colors.grey.shade300,
                    width: isSelected ? 2.5 : 1,
                  ),
                ),
                child: Row(
                  children: [
                    CircleAvatar(
                      radius: 30,
                      backgroundImage: staff.avatar != null && staff.avatar!.isNotEmpty
                          ? NetworkImage(staff.avatar!)
                          : null,
                      child: staff.avatar == null || staff.avatar!.isEmpty
                          ? const Icon(Icons.person, size: 30)
                          : null,
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text('Dr. ${staff.fullName}', style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15)),
                          const SizedBox(height: 4),
                          Row(
                            children: [
                              const Icon(Icons.star, color: Colors.amber, size: 16),
                              const SizedBox(width: 4),
                              Text('${staff.experienceYears ?? 0} Yrs Experience', style: const TextStyle(fontSize: 12, color: Colors.grey)),
                            ],
                          )
                        ],
                      ),
                    ),
                    IconButton(
                      icon: const Icon(Icons.info_outline, color: Colors.grey),
                      onPressed: () => _showStaffDetailModal(staff),
                    ),
                    if (isSelected)
                      Icon(Icons.check_circle, color: Theme.of(context).primaryColor, size: 24),
                  ],
                ),
              ),
            );
          },
        ),
      ],
    );
  }

  // ==========================================
  // STEP 3: CARE SERVICES (Max 2 & Same ServiceType)
  // ==========================================
  Widget _buildStep3Services() {
    final services = _bookingData?.services ?? [];

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text('Care Services', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
        const Text('You can select a maximum of 2 services of the same category.', style: TextStyle(color: Colors.grey, fontSize: 13)),
        const SizedBox(height: 16),
        ListView.separated(
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          itemCount: services.length,
          separatorBuilder: (_, __) => const SizedBox(height: 12),
          itemBuilder: (context, index) {
            final service = services[index];
            final isSelected = _selectedServices.any((s) => s.serviceId == service.serviceId);

            return InkWell(
              onTap: () {
                setState(() {
                  if (isSelected) {
                    _selectedServices.removeWhere((s) => s.serviceId == service.serviceId);
                  } else {
                    // Validation 1: Maximum 2 services
                    if (_selectedServices.length >= 2) {
                      _showToast('You can only select a maximum of 2 services per appointment.');
                      return;
                    }
                    // Validation 2: Must be of the same ServiceType
                    if (_selectedServices.isNotEmpty) {
                      final currentType = _selectedServices.first.serviceType;
                      if (service.serviceType != currentType) {
                        _showToast('You can only select services of the same category.');
                        return;
                      }
                    }
                    _selectedServices.add(service);
                  }
                  // Reset slots when services change
                  _selectedSlot = null;
                  _availableSlots = [];
                  _hasSearchedSlots = false;
                });
              },
              borderRadius: BorderRadius.circular(16),
              child: Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(16),
                  border: Border.all(
                    color: isSelected ? Theme.of(context).primaryColor : Colors.grey.shade300,
                    width: isSelected ? 2.5 : 1,
                  ),
                ),
                child: Row(
                  children: [
                    ClipRRect(
                      borderRadius: BorderRadius.circular(8),
                      child: service.serviceImages.isNotEmpty
                          ? Image.network(service.serviceImages.first, width: 60, height: 60, fit: BoxFit.cover)
                          : Container(width: 60, height: 60, color: Colors.grey.shade200, child: const Icon(Icons.medical_services)),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(service.serviceName, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14)),
                          const SizedBox(height: 4),
                          Row(
                            children: [
                              Text('${NumberFormat('#,###', 'vi_VN').format(service.price)}đ', style: TextStyle(fontWeight: FontWeight.bold, color: Colors.green.shade700)),
                              const SizedBox(width: 12),
                              const Icon(Icons.access_time, size: 14, color: Colors.grey),
                              const SizedBox(width: 2),
                              Text('${service.duration} mins', style: const TextStyle(fontSize: 12, color: Colors.grey)),
                            ],
                          )
                        ],
                      ),
                    ),
                    Checkbox(
                      value: isSelected,
                      onChanged: null,
                      activeColor: Theme.of(context).primaryColor,
                    )
                  ],
                ),
              ),
            );
          },
        ),
      ],
    );
  }

  // ==========================================
  // STEP 4: DATE & HORIZONTAL SCROLLABLE TIME SLOTS
  // ==========================================
  Widget _buildStep4Slots() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text('Appointment Schedule', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
        const Text('Pick your preferred date and find an available time slot.', style: TextStyle(color: Colors.grey, fontSize: 13)),
        const SizedBox(height: 16),
        
        // Select Date
        Row(
          children: [
            Expanded(
              child: Container(
                padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                decoration: BoxDecoration(
                  border: Border.all(color: Colors.grey.shade400),
                  borderRadius: BorderRadius.circular(12),
                  color: Colors.white,
                ),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text(DateFormat('dd/MM/yyyy').format(_selectedDate), style: const TextStyle(fontWeight: FontWeight.bold)),
                    const Icon(Icons.calendar_today, size: 20, color: Colors.grey),
                  ],
                ),
              ),
            ),
            const SizedBox(width: 8),
            ElevatedButton(
              onPressed: () async {
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
                }
              },
              style: ElevatedButton.styleFrom(
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                padding: const EdgeInsets.symmetric(vertical: 14, horizontal: 16),
              ),
              child: const Text('Change'),
            )
          ],
        ),
        const SizedBox(height: 12),
        SizedBox(
          width: double.infinity,
          child: ElevatedButton.icon(
            onPressed: _loadAvailableSlots,
            icon: const Icon(Icons.search),
            label: const Text('Find Slots', style: TextStyle(fontWeight: FontWeight.bold)),
            style: ElevatedButton.styleFrom(
              padding: const EdgeInsets.symmetric(vertical: 12),
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
            ),
          ),
        ),
        const SizedBox(height: 20),

        // Title Header
        const Text('Available Time Slots:', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 14)),
        const SizedBox(height: 10),

        // 🟢 HORIZONTAL SCROLLABLE TIME SLOTS
        if (_isLoadingSlots)
          const Center(child: Padding(padding: EdgeInsets.all(20), child: CircularProgressIndicator()))
        else if (!_hasSearchedSlots)
          Container(
            width: double.infinity,
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(color: Colors.grey.shade100, borderRadius: BorderRadius.circular(12)),
            child: const Text('Please set a date above and click "Find Slots".', textAlign: TextAlign.center, style: TextStyle(color: Colors.grey)),
          )
        else if (_availableSlots.isEmpty)
          Container(
            width: double.infinity,
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(color: Colors.amber.shade50, borderRadius: BorderRadius.circular(12)),
            child: const Text('Fully booked or no matching hours on this day.', textAlign: TextAlign.center, style: TextStyle(color: Colors.amber)),
          )
        else
          SizedBox(
            height: 72, // Chiều cao vừa vặn cho card time slot
            child: ListView.builder(
              scrollDirection: Axis.horizontal,
              physics: const BouncingScrollPhysics(), // Hiệu ứng cuộn nẩy mượt mà
              itemCount: _availableSlots.length,
              itemBuilder: (context, index) {
                final slot = _availableSlots[index];
                final isSelected = _selectedSlot == slot;
                final timeText = '${DateFormat('hh:mm a').format(slot.startTime)} - ${DateFormat('hh:mm a').format(slot.endTime)}';

                return Padding(
                  padding: const EdgeInsets.only(right: 10.0),
                  child: InkWell(
                    onTap: () {
                      setState(() {
                        _selectedSlot = slot;
                      });
                    },
                    borderRadius: BorderRadius.circular(12),
                    child: Container(
                      width: 155,
                      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 8),
                      decoration: BoxDecoration(
                        color: isSelected
                            ? Theme.of(context).primaryColor
                            : slot.isRecommended
                                ? Colors.amber.shade50
                                : Colors.white,
                        border: Border.all(
                          color: isSelected
                              ? Theme.of(context).primaryColor
                              : slot.isRecommended
                                  ? Colors.amber.shade700
                                  : Colors.grey.shade300,
                          width: 1.5,
                        ),
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Text(
                            timeText,
                            style: TextStyle(
                              fontWeight: FontWeight.bold,
                              fontSize: 12,
                              color: isSelected ? Colors.white : Colors.black87,
                            ),
                            textAlign: TextAlign.center,
                          ),
                          if (slot.isRecommended) ...[
                            const SizedBox(height: 4),
                            Text(
                              '⭐ #${slot.recommendationRank ?? 1} Recommended',
                              style: TextStyle(
                                fontSize: 10,
                                color: isSelected ? Colors.white70 : Colors.amber.shade900,
                                fontWeight: FontWeight.bold,
                              ),
                            )
                          ]
                        ],
                      ),
                    ),
                  ),
                );
              },
            ),
          ),

        const SizedBox(height: 20),
        // Notes for Doctor
        TextField(
          controller: _noteController,
          maxLines: 3,
          decoration: InputDecoration(
            labelText: 'Notes for the Doctor (Optional)',
            hintText: 'Describe any symptoms or requirements here...',
            border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
          ),
        ),
      ],
    );
  }

  // ==========================================
  // BOOKING SUMMARY CARD
  // ==========================================
  Widget _buildSummaryCard() {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(color: Colors.black.withOpacity(0.05), blurRadius: 10, offset: const Offset(0, 4)),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('Booking Summary', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
          const Divider(height: 20),
          _buildSummaryRow('Pet Patient:', _selectedPet?.petName ?? '—'),
          const SizedBox(height: 6),
          _buildSummaryRow('Doctor:', _selectedStaff != null ? 'Dr. ${_selectedStaff!.fullName}' : '—'),
          const SizedBox(height: 6),
          _buildSummaryRow('Services:', _selectedServices.isNotEmpty ? _selectedServices.map((s) => s.serviceName).join(', ') : '—'),
          const SizedBox(height: 6),
          _buildSummaryRow('Duration:', '$_totalDuration minutes'),
          if (_selectedSlot != null) ...[
            const SizedBox(height: 6),
            _buildSummaryRow(
              'Schedule:',
              '${DateFormat('dd/MM/yyyy').format(_selectedDate)} @ ${DateFormat('hh:mm a').format(_selectedSlot!.startTime)}',
              valueColor: Colors.blue.shade800,
            ),
          ],
          const Divider(height: 20),
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              const Text('Total Price:', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 15)),
              Text(
                '${NumberFormat('#,###', 'vi_VN').format(_totalPrice)} đ',
                style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18, color: Colors.green.shade700),
              )
            ],
          )
        ],
      ),
    );
  }

  Widget _buildSummaryRow(String label, String value, {Color? valueColor}) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(width: 90, child: Text(label, style: const TextStyle(color: Colors.grey, fontSize: 13))),
        Expanded(
          child: Text(
            value,
            style: TextStyle(fontWeight: FontWeight.w600, fontSize: 13, color: valueColor ?? Colors.black87),
          ),
        )
      ],
    );
  }

  // ==========================================
  // BOTTOM NAVIGATION BAR
  // ==========================================
  Widget _buildBottomNavigation() {
    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: Colors.white,
        boxShadow: [
          BoxShadow(color: Colors.black.withOpacity(0.05), blurRadius: 10, offset: const Offset(0, -2)),
        ],
      ),
      child: Row(
        children: [
          if (_currentStep > 1)
            Expanded(
              child: OutlinedButton(
                onPressed: _prevStep,
                style: OutlinedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 14),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                ),
                child: const Text('Back'),
              ),
            ),
          if (_currentStep > 1) const SizedBox(width: 12),
          Expanded(
            child: ElevatedButton(
              onPressed: _currentStep == 4 ? _submitBooking : _nextStep,
              style: ElevatedButton.styleFrom(
                backgroundColor: Theme.of(context).primaryColor,
                padding: const EdgeInsets.symmetric(vertical: 14),
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
              ),
              child: Text(
                _currentStep == 4 ? 'CONFIRM & BOOK' : 'Next Step',
                style: const TextStyle(fontWeight: FontWeight.bold, color: Colors.white),
              ),
            ),
          ),
        ],
      ),
    );
  }

  // ==========================================
  // ENTITY DETAIL MODALS
  // ==========================================
  void _showPetDetailModal(BookingPetModel pet) {
    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (_) => Padding(
        padding: const EdgeInsets.all(20.0),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            CircleAvatar(
              radius: 40,
              backgroundImage: pet.petAvatar != null && pet.petAvatar!.isNotEmpty ? NetworkImage(pet.petAvatar!) : null,
              child: pet.petAvatar == null || pet.petAvatar!.isEmpty ? const Icon(Icons.pets, size: 40) : null,
            ),
            const SizedBox(height: 12),
            Text(pet.petName, style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
            const SizedBox(height: 16),
            ListTile(title: const Text('Species'), trailing: Text(pet.species ?? 'N/A')),
            ListTile(title: const Text('Breed'), trailing: Text(pet.breed ?? 'N/A')),
            ListTile(title: const Text('Gender'), trailing: Text(pet.gender ?? 'N/A')),
            ListTile(title: const Text('Weight'), trailing: Text(pet.weight != null ? '${pet.weight} kg' : 'N/A')),
          ],
        ),
      ),
    );
  }

  void _showStaffDetailModal(BookingStaffModel staff) {
    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (_) => Padding(
        padding: const EdgeInsets.all(20.0),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            CircleAvatar(
              radius: 40,
              backgroundImage: staff.avatar != null && staff.avatar!.isNotEmpty ? NetworkImage(staff.avatar!) : null,
              child: staff.avatar == null || staff.avatar!.isEmpty ? const Icon(Icons.person, size: 40) : null,
            ),
            const SizedBox(height: 12),
            Text('Dr. ${staff.fullName}', style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
            if (staff.description != null) ...[
              const SizedBox(height: 6),
              Text(staff.description!, style: const TextStyle(color: Colors.grey, fontStyle: FontStyle.italic), textAlign: TextAlign.center),
            ],
            const SizedBox(height: 16),
            ListTile(title: const Text('Experience'), trailing: Text('${staff.experienceYears ?? 0} Years')),
            ListTile(title: const Text('License Number'), trailing: Text(staff.licenseNumber ?? 'N/A')),
            ListTile(title: const Text('Email'), trailing: Text(staff.email)),
            ListTile(title: const Text('Hotline'), trailing: Text(staff.phoneNumber)),
          ],
        ),
      ),
    );
  }
}