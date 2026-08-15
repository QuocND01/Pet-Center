import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../../constants/app_colors.dart';
import '../../../models/appointment_model.dart';
import '../../../services/api_service.dart';
import 'appointment_detail_screen.dart';
import 'booking_screen.dart';

class AppointmentListScreen extends StatefulWidget {
  const AppointmentListScreen({super.key});

  @override
  State<AppointmentListScreen> createState() => _AppointmentListScreenState();
}

class _AppointmentListScreenState extends State<AppointmentListScreen> with SingleTickerProviderStateMixin {
  final ApiService _apiService = ApiService();
  late TabController _tabController;
  late Future<List<AppointmentListModel>> _appointmentsFuture;
  List<AppointmentListModel> _allAppointments = [];
  DateTime? _selectedDate; // Ngày được chọn để lọc (null = tất cả ngày)

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 5, vsync: this);
    _loadAppointments();
  }

  void _loadAppointments() {
    setState(() {
      _appointmentsFuture = _apiService.getMyAppointments().then((list) {
        _allAppointments = list;
        return list;
      });
    });
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  // Lọc kết hợp đồng thời cả Status và Date
  List<AppointmentListModel> _filterAppointments(int? filterStatus) {
    return _allAppointments.where((a) {
      // 1. Kiểm tra Status (null = All)
      final matchesStatus = filterStatus == null || a.status == filterStatus;

      // 2. Kiểm tra Date (null = All dates)
      final matchesDate = _selectedDate == null ||
          (a.appointmentStart.year == _selectedDate!.year &&
              a.appointmentStart.month == _selectedDate!.month &&
              a.appointmentStart.day == _selectedDate!.day);

      return matchesStatus && matchesDate;
    }).toList();
  }

  Color _getStatusColor(int status) {
    switch (status) {
      case 1:
        return Colors.orange; // Reserved / Pending
      case 2:
        return Colors.blue; // Confirmed
      case 3:
        return Colors.green; // Completed
      case 0:
        return Colors.red; // Cancelled
      default:
        return Colors.grey;
    }
  }

  String _getStatusText(int status, String? originalText) {
    switch (status) {
      case 0:
        return 'Cancelled';
      case 1:
        return 'Reserved';
      case 2:
        return 'Confirmed';
      case 3:
        return 'Completed';
      default:
        return (originalText != null && originalText.isNotEmpty && originalText != 'Unknown')
            ? originalText
            : 'Unknown';
    }
  }

  // Mở Date Picker để chọn ngày lọc
  Future<void> _pickFilterDate() async {
    final picked = await showDatePicker(
      context: context,
      initialDate: _selectedDate ?? DateTime.now(),
      firstDate: DateTime(2020),
      lastDate: DateTime(2035),
    );

    if (picked != null) {
      setState(() {
        _selectedDate = picked;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('My Appointments'),
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
        actions: [
          // Nút chọn ngày trên AppBar
          IconButton(
            icon: Icon(
              _selectedDate != null ? Icons.filter_alt : Icons.calendar_month,
              color: _selectedDate != null ? Colors.amberAccent : Colors.white,
            ),
            tooltip: 'Filter by Date',
            onPressed: _pickFilterDate,
          ),
          if (_selectedDate != null)
            IconButton(
              icon: const Icon(Icons.clear, color: Colors.white),
              tooltip: 'Clear Date Filter',
              onPressed: () {
                setState(() {
                  _selectedDate = null;
                });
              },
            ),
        ],
        bottom: TabBar(
          controller: _tabController,
          isScrollable: true,
          labelColor: Colors.white,
          unselectedLabelColor: Colors.white70,
          indicatorColor: Colors.white,
          tabs: const [
            Tab(text: 'All'),
            Tab(text: 'Reserved'),
            Tab(text: 'Confirmed'),
            Tab(text: 'Completed'),
            Tab(text: 'Cancelled'),
          ],
        ),
      ),
      body: Column(
        children: [
          // Thanh hiển thị ngày đang được chọn lọc (nếu có)
          if (_selectedDate != null)
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
              color: AppColors.primary.withAlpha(20),
              child: Row(
                children: [
                  const Icon(Icons.date_range, size: 18, color: AppColors.primary),
                  const SizedBox(width: 8),
                  Text(
                    'Filtering date: ${DateFormat('dd/MM/yyyy').format(_selectedDate!)}',
                    style: const TextStyle(fontWeight: FontWeight.bold, color: AppColors.primary, fontSize: 13),
                  ),
                  const Spacer(),
                  InkWell(
                    onTap: () {
                      setState(() {
                        _selectedDate = null;
                      });
                    },
                    child: const Text(
                      'Clear',
                      style: TextStyle(color: Colors.red, fontWeight: FontWeight.bold, fontSize: 13),
                    ),
                  ),
                ],
              ),
            ),

          // Danh sách lịch hẹn theo Tab
          Expanded(
            child: FutureBuilder<List<AppointmentListModel>>(
              future: _appointmentsFuture,
              builder: (context, snapshot) {
                if (snapshot.connectionState == ConnectionState.waiting && _allAppointments.isEmpty) {
                  return const Center(child: CircularProgressIndicator());
                }

                if (snapshot.hasError) {
                  return Center(
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        const Icon(Icons.error_outline, size: 64, color: AppColors.error),
                        const SizedBox(height: 16),
                        Text('Failed to load appointments: ${snapshot.error}', textAlign: TextAlign.center),
                        const SizedBox(height: 16),
                        ElevatedButton(onPressed: _loadAppointments, child: const Text('Reload')),
                      ],
                    ),
                  );
                }

                return TabBarView(
                  controller: _tabController,
                  children: [
                    _buildAppointmentList(_filterAppointments(null)), // All
                    _buildAppointmentList(_filterAppointments(1)),    // Reserved
                    _buildAppointmentList(_filterAppointments(2)),    // Confirmed
                    _buildAppointmentList(_filterAppointments(3)),    // Completed
                    _buildAppointmentList(_filterAppointments(0)),    // Cancelled
                  ],
                );
              },
            ),
          ),
        ],
      ),
      floatingActionButton: FloatingActionButton.extended(
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
        icon: const Icon(Icons.calendar_today),
        label: const Text('Book Appointment'),
        onPressed: () {
          Navigator.push(
            context,
            MaterialPageRoute(builder: (context) => const BookingScreen()),
          ).then((_) => _loadAppointments());
        },
      ),
    );
  }

  Widget _buildAppointmentList(List<AppointmentListModel> list) {
    if (list.isEmpty) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.event_busy, size: 64, color: Colors.grey),
            const SizedBox(height: 16),
            Text(
              _selectedDate != null
                  ? 'No appointments found on ${DateFormat('dd/MM/yyyy').format(_selectedDate!)}'
                  : 'No appointments found.',
              style: const TextStyle(color: AppColors.textSecondary),
            ),
            if (_selectedDate != null) ...[
              const SizedBox(height: 12),
              OutlinedButton.icon(
                icon: const Icon(Icons.clear, size: 16),
                label: const Text('Clear Date Filter'),
                onPressed: () {
                  setState(() {
                    _selectedDate = null;
                  });
                },
              ),
            ],
          ],
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: () async => _loadAppointments(),
      child: ListView.builder(
        padding: const EdgeInsets.all(16),
        itemCount: list.length,
        itemBuilder: (context, index) {
          final item = list[index];
          final color = _getStatusColor(item.status);
          final statusText = _getStatusText(item.status, item.statusText);
          final dateStr = DateFormat('EEE, dd/MM/yyyy HH:mm').format(item.appointmentStart);

          return Card(
            margin: const EdgeInsets.only(bottom: 16),
            elevation: 2,
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
            child: InkWell(
              borderRadius: BorderRadius.circular(12),
              onTap: () {
                Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (context) => AppointmentDetailScreen(appointmentId: item.appointmentId),
                  ),
                ).then((_) => _loadAppointments());
              },
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Row(
                          children: [
                            const Icon(Icons.pets, color: AppColors.primary, size: 20),
                            const SizedBox(width: 8),
                            Text(
                              item.petName,
                              style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                            ),
                          ],
                        ),
                        Container(
                          padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                          decoration: BoxDecoration(
                            color: color.withAlpha(30),
                            borderRadius: BorderRadius.circular(12),
                            border: Border.all(color: color),
                          ),
                          child: Text(
                            statusText,
                            style: TextStyle(color: color, fontSize: 12, fontWeight: FontWeight.bold),
                          ),
                        ),
                      ],
                    ),
                    const Divider(height: 20),
                    Row(
                      children: [
                        const Icon(Icons.person_outline, size: 18, color: AppColors.textSecondary),
                        const SizedBox(width: 6),
                        Text('Doctor/Vet: ${item.vetName.isNotEmpty ? item.vetName : "Assigned Vet"}'),
                      ],
                    ),
                    const SizedBox(height: 6),
                    Row(
                      children: [
                        const Icon(Icons.access_time, size: 18, color: AppColors.textSecondary),
                        const SizedBox(width: 6),
                        Text(dateStr, style: const TextStyle(fontWeight: FontWeight.w500)),
                      ],
                    ),
                    const Divider(height: 20),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        const Text('Total:', style: TextStyle(color: AppColors.textSecondary)),
                        Text(
                          '${item.total.toStringAsFixed(0)}đ',
                          style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: AppColors.primary),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ),
          );
        },
      ),
    );
  }
}