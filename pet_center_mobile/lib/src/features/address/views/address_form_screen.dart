import 'package:flutter/material.dart';
import '../../../constants/app_colors.dart';
import '../../../models/address_model.dart';
import '../../../services/api_service.dart';

class AddressFormScreen extends StatefulWidget {
  final AddressModel? address;

  const AddressFormScreen({super.key, this.address});

  @override
  State<AddressFormScreen> createState() => _AddressFormScreenState();
}

class _AddressFormScreenState extends State<AddressFormScreen> {
  final _formKey = GlobalKey<FormState>();
  final ApiService _apiService = ApiService();

  late TextEditingController _detailsController;
  late TextEditingController _wardController;
  late TextEditingController _districtController;
  late TextEditingController _provinceController;
  bool _isDefault = false;
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    _detailsController =
        TextEditingController(text: widget.address?.addressDetails ?? '');
    _wardController = TextEditingController(text: widget.address?.ward ?? '');
    _districtController =
        TextEditingController(text: widget.address?.district ?? '');
    _provinceController =
        TextEditingController(text: widget.address?.province ?? '');
    _isDefault = widget.address?.isDefault ?? false;
  }

  @override
  void dispose() {
    _detailsController.dispose();
    _wardController.dispose();
    _districtController.dispose();
    _provinceController.dispose();
    super.dispose();
  }

  String _sanitizeInput(String val) {
    return val.replaceAll(RegExp(r'[^\p{L}\p{N}\s,.\-\/]', unicode: true), '');
  }

  Future<void> _submitForm() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _isLoading = true;
    });

    try {
      final details = _sanitizeInput(_detailsController.text.trim());
      final ward = _sanitizeInput(_wardController.text.trim());
      final district = _sanitizeInput(_districtController.text.trim());
      final province = _sanitizeInput(_provinceController.text.trim());

      bool success = false;
      if (widget.address != null) {
        success = await _apiService.updateAddress(
          addressId: widget.address!.addressId,
          province: province,
          district: district,
          ward: ward,
          addressDetails: details,
          isDefault: _isDefault,
        );
      } else {
        success = await _apiService.addAddress(
          province: province,
          district: district,
          ward: ward,
          addressDetails: details,
          isDefault: _isDefault,
        );
      }

      if (!mounted) return;

      if (success) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(
              widget.address != null
                  ? 'Address updated successfully.'
                  : 'New address added successfully.',
            ),
            backgroundColor: AppColors.success,
          ),
        );
        Navigator.pop(context, true);
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Failed to save address. Please try again.'),
            backgroundColor: AppColors.error,
          ),
        );
      }
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Error: $e'),
          backgroundColor: AppColors.error,
        ),
      );
    } finally {
      if (mounted) {
        setState(() {
          _isLoading = false;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final isEditing = widget.address != null;

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Text(isEditing ? 'Edit Address' : 'Add New Address'),
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
        elevation: 0,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16.0),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Card(
                shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12)),
                elevation: 1,
                child: Padding(
                  padding: const EdgeInsets.all(16.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Text(
                        'Address Details',
                        style: TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.bold,
                          color: AppColors.primary,
                        ),
                      ),
                      const SizedBox(height: 16),
                      TextFormField(
                        controller: _detailsController,
                        decoration: InputDecoration(
                          labelText: 'Street Name, House No. *',
                          hintText: 'e.g., 123 Nguyen Van Cu',
                          prefixIcon: const Icon(Icons.home_outlined,
                              color: AppColors.primary),
                          border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(10)),
                        ),
                        validator: (val) {
                          if (val == null || val.trim().isEmpty) {
                            return 'Street details are required.';
                          }
                          return null;
                        },
                      ),
                      const SizedBox(height: 16),
                      TextFormField(
                        controller: _wardController,
                        decoration: InputDecoration(
                          labelText: 'Ward / Sub-district',
                          hintText: 'e.g., An Hoa',
                          prefixIcon: const Icon(Icons.map_outlined,
                              color: AppColors.primary),
                          border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(10)),
                        ),
                      ),
                      const SizedBox(height: 16),
                      TextFormField(
                        controller: _districtController,
                        decoration: InputDecoration(
                          labelText: 'District *',
                          hintText: 'e.g., Ninh Kieu',
                          prefixIcon: const Icon(Icons.location_city_outlined,
                              color: AppColors.primary),
                          border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(10)),
                        ),
                        validator: (val) {
                          if (val == null || val.trim().isEmpty) {
                            return 'District is required.';
                          }
                          return null;
                        },
                      ),
                      const SizedBox(height: 16),
                      TextFormField(
                        controller: _provinceController,
                        decoration: InputDecoration(
                          labelText: 'Province / City *',
                          hintText: 'e.g., Can Tho',
                          prefixIcon: const Icon(Icons.nature_people_outlined,
                              color: AppColors.primary),
                          border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(10)),
                        ),
                        validator: (val) {
                          if (val == null || val.trim().isEmpty) {
                            return 'Province / City is required.';
                          }
                          return null;
                        },
                      ),
                      const SizedBox(height: 16),
                      SwitchListTile(
                        activeThumbColor: AppColors.primary,
                        contentPadding: EdgeInsets.zero,
                        title: const Text(
                          'Set as default shipping address',
                          style: TextStyle(
                              fontSize: 14, fontWeight: FontWeight.w600),
                        ),
                        value: _isDefault,
                        onChanged: (val) {
                          setState(() {
                            _isDefault = val;
                          });
                        },
                      ),
                    ],
                  ),
                ),
              ),
              const SizedBox(height: 24),
              SizedBox(
                width: double.infinity,
                height: 50,
                child: ElevatedButton.icon(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppColors.primary,
                    foregroundColor: Colors.white,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
                    ),
                  ),
                  icon: _isLoading
                      ? const SizedBox(
                          width: 20,
                          height: 20,
                          child: CircularProgressIndicator(
                              color: Colors.white, strokeWidth: 2),
                        )
                      : const Icon(Icons.save_outlined),
                  label: Text(
                    _isLoading
                        ? 'Saving...'
                        : (isEditing ? 'Update Address' : 'Save Address'),
                    style: const TextStyle(
                        fontSize: 16, fontWeight: FontWeight.bold),
                  ),
                  onPressed: _isLoading ? null : _submitForm,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
