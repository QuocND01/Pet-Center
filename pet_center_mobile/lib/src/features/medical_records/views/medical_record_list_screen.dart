import 'package:flutter/material.dart';
import '../../../constants/app_colors.dart';
import '../../../models/medical_record_model.dart';
import '../../../services/api_service.dart';
import '../widgets/medical_record_detail_sheet.dart';

class MedicalRecordListScreen extends StatefulWidget {
  const MedicalRecordListScreen({super.key});

  @override
  State<MedicalRecordListScreen> createState() => _MedicalRecordListScreenState();
}

class _MedicalRecordListScreenState extends State<MedicalRecordListScreen> {
  final ApiService _apiService = ApiService();
  final TextEditingController _searchController = TextEditingController();

  late Future<List<MedicalRecordModel>> _recordsFuture;
  List<MedicalRecordModel> _allRecords = [];
  String _searchQuery = '';
  int _selectedStatusFilter = -1; // -1 = All, 0 = Drafted, 1 = Completed

  @override
  void initState() {
    super.initState();
    _loadRecords();
  }

  void _loadRecords() {
    setState(() {
      _recordsFuture = _apiService.getMyMedicalRecords(search: _searchQuery).then((list) {
        if (mounted) {
          setState(() {
            _allRecords = list;
          });
        }
        return list;
      });
    });
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  String _formatDate(DateTime? dt) {
    if (dt == null) return 'N/A';
    final d = dt.day.toString().padLeft(2, '0');
    final m = dt.month.toString().padLeft(2, '0');
    final y = dt.year;
    return '$d/$m/$y';
  }

  Color _getStatusBgColor(String statusName) {
    final name = statusName.toLowerCase();
    if (name.contains('complete')) return const Color(0xFFD1FAE5);
    if (name.contains('draft')) return const Color(0xFFFEF3C7);
    if (name.contains('cancel')) return const Color(0xFFFEE2E2);
    return Colors.grey.shade100;
  }

  Color _getStatusTextColor(String statusName) {
    final name = statusName.toLowerCase();
    if (name.contains('complete')) return const Color(0xFF065F46);
    if (name.contains('draft')) return const Color(0xFF92400E);
    if (name.contains('cancel')) return const Color(0xFF991B1B);
    return Colors.grey.shade800;
  }

  List<MedicalRecordModel> get _filteredRecords {
    if (_selectedStatusFilter == -1) return _allRecords;
    return _allRecords.where((r) {
      final name = r.statusName.toLowerCase();
      if (_selectedStatusFilter == 1) return name.contains('complete');
      if (_selectedStatusFilter == 0) return name.contains('draft');
      return true;
    }).toList();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Medical History & Prescriptions', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
        backgroundColor: Colors.white,
        foregroundColor: AppColors.textPrimary,
        elevation: 0,
        centerTitle: false,
      ),
      body: Column(
        children: [
          // Search & Filter Header Container
          Container(
            color: Colors.white,
            padding: const EdgeInsets.fromLTRB(16, 8, 16, 12),
            child: Column(
              children: [
                // Search Field
                TextField(
                  controller: _searchController,
                  onChanged: (val) {
                    _searchQuery = val;
                    _loadRecords();
                  },
                  decoration: InputDecoration(
                    hintText: 'Search by diagnosis, vet, or pet...',
                    hintStyle: const TextStyle(fontSize: 13, color: Colors.grey),
                    prefixIcon: const Icon(Icons.search, color: Colors.grey, size: 20),
                    suffixIcon: _searchQuery.isNotEmpty
                        ? IconButton(
                            icon: const Icon(Icons.clear, size: 18, color: Colors.grey),
                            onPressed: () {
                              _searchController.clear();
                              _searchQuery = '';
                              _loadRecords();
                            },
                          )
                        : null,
                    filled: true,
                    fillColor: Colors.grey.shade100,
                    contentPadding: const EdgeInsets.symmetric(vertical: 0, horizontal: 16),
                    border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12),
                      borderSide: BorderSide.none,
                    ),
                  ),
                ),
                const SizedBox(height: 10),

                // Status Filter Chips
                SingleChildScrollView(
                  scrollDirection: Axis.horizontal,
                  child: Row(
                    children: [
                      _buildFilterChip('All Records', -1),
                      const SizedBox(width: 8),
                      _buildFilterChip('Completed', 1),
                      const SizedBox(width: 8),
                      _buildFilterChip('Drafted', 0),
                    ],
                  ),
                ),
              ],
            ),
          ),
          const Divider(height: 1),

          // Record List
          Expanded(
            child: RefreshIndicator(
              onRefresh: () async => _loadRecords(),
              color: AppColors.primary,
              child: FutureBuilder<List<MedicalRecordModel>>(
                future: _recordsFuture,
                builder: (context, snapshot) {
                  if (snapshot.connectionState == ConnectionState.waiting && _allRecords.isEmpty) {
                    return const Center(child: CircularProgressIndicator(color: AppColors.primary));
                  }

                  if (snapshot.hasError && _allRecords.isEmpty) {
                    return Center(
                      child: Padding(
                        padding: const EdgeInsets.all(24.0),
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            const Icon(Icons.error_outline_rounded, color: AppColors.error, size: 48),
                            const SizedBox(height: 12),
                            Text('Error: ${snapshot.error}', style: const TextStyle(color: AppColors.error), textAlign: TextAlign.center),
                            const SizedBox(height: 16),
                            ElevatedButton(
                              onPressed: _loadRecords,
                              style: ElevatedButton.styleFrom(backgroundColor: AppColors.primary),
                              child: const Text('Try Again', style: TextStyle(color: Colors.white)),
                            ),
                          ],
                        ),
                      ),
                    );
                  }

                  final list = _filteredRecords;
                  if (list.isEmpty) {
                    return ListView(
                      children: [
                        const SizedBox(height: 80),
                        Center(
                          child: Column(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              Container(
                                padding: const EdgeInsets.all(20),
                                decoration: BoxDecoration(
                                  color: Colors.teal.shade50,
                                  shape: BoxShape.circle,
                                ),
                                child: const Icon(Icons.medical_services_outlined, size: 48, color: Colors.teal),
                              ),
                              const SizedBox(height: 16),
                              const Text(
                                'No Medical Records Found',
                                style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: AppColors.textPrimary),
                              ),
                              const SizedBox(height: 6),
                              const Padding(
                                padding: EdgeInsets.symmetric(horizontal: 32),
                                child: Text(
                                  'Your pet\'s medical examination records and prescriptions will appear here.',
                                  textAlign: TextAlign.center,
                                  style: TextStyle(fontSize: 13, color: AppColors.textSecondary),
                                ),
                              ),
                            ],
                          ),
                        ),
                      ],
                    );
                  }

                  return ListView.separated(
                    padding: const EdgeInsets.all(16),
                    itemCount: list.length,
                    separatorBuilder: (ctx, idx) => const SizedBox(height: 14),
                    itemBuilder: (ctx, idx) {
                      final record = list[idx];
                      return _buildRecordCard(record);
                    },
                  );
                },
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildFilterChip(String label, int statusValue) {
    final isSelected = _selectedStatusFilter == statusValue;
    return ChoiceChip(
      label: Text(label),
      selected: isSelected,
      onSelected: (selected) {
        if (selected) {
          setState(() {
            _selectedStatusFilter = statusValue;
          });
        }
      },
      selectedColor: AppColors.primary,
      backgroundColor: Colors.grey.shade100,
      labelStyle: TextStyle(
        fontSize: 12,
        fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
        color: isSelected ? Colors.white : AppColors.textSecondary,
      ),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(100)),
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
    );
  }

  Widget _buildRecordCard(MedicalRecordModel record) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: AppColors.inputBorder),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withAlpha(6),
            blurRadius: 6,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Meta Header: Date & Status Badge
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Row(
                children: [
                  const Icon(Icons.calendar_today_rounded, size: 14, color: AppColors.textSecondary),
                  const SizedBox(width: 6),
                  Text(
                    _formatDate(record.appointmentStart),
                    style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w600, color: AppColors.textSecondary),
                  ),
                ],
              ),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 3),
                decoration: BoxDecoration(
                  color: _getStatusBgColor(record.statusName),
                  borderRadius: BorderRadius.circular(100),
                ),
                child: Text(
                  record.statusName,
                  style: TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: _getStatusTextColor(record.statusName)),
                ),
              ),
            ],
          ),
          const SizedBox(height: 10),

          // Vet & Pet Badges
          Row(
            children: [
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                decoration: BoxDecoration(
                  color: Colors.indigo.shade50,
                  borderRadius: BorderRadius.circular(6),
                ),
                child: Row(
                  children: [
                    const Icon(Icons.person_pin_rounded, size: 13, color: Colors.indigo),
                    const SizedBox(width: 4),
                    Text(
                      record.vetName,
                      style: const TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: Colors.indigo),
                    ),
                  ],
                ),
              ),
              const SizedBox(width: 8),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                decoration: BoxDecoration(
                  color: Colors.pink.shade50,
                  borderRadius: BorderRadius.circular(6),
                ),
                child: Row(
                  children: [
                    const Icon(Icons.pets, size: 13, color: Colors.pink),
                    const SizedBox(width: 4),
                    Text(
                      '${record.petSpecies} - ${record.petBreed}',
                      style: const TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: Colors.pink),
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),

          // Diagnosis Title
          Text(
            record.diagnosis,
            style: const TextStyle(fontSize: 15, fontWeight: FontWeight.bold, color: AppColors.textPrimary),
          ),
          const SizedBox(height: 4),

          // Treatment Snippet
          Text(
            record.treatment,
            style: const TextStyle(fontSize: 13, color: AppColors.textSecondary, height: 1.3),
            maxLines: 2,
            overflow: TextOverflow.ellipsis,
          ),
          const Divider(height: 20),

          // View Details Action Button
          SizedBox(
            width: double.infinity,
            height: 40,
            child: OutlinedButton.icon(
              style: OutlinedButton.styleFrom(
                foregroundColor: AppColors.primary,
                side: const BorderSide(color: AppColors.primary, width: 1.2),
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
              ),
              onPressed: () {
                showModalBottomSheet(
                  context: context,
                  isScrollControlled: true,
                  backgroundColor: Colors.transparent,
                  builder: (ctx) => MedicalRecordDetailSheet(recordId: record.recordId),
                );
              },
              icon: const Icon(Icons.visibility_outlined, size: 18),
              label: const Text(
                'View Details & Prescription',
                style: TextStyle(fontWeight: FontWeight.bold, fontSize: 13),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
