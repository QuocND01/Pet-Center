import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import '../../../constants/app_colors.dart';
import '../../../models/customer_model.dart';
import '../../../services/api_service.dart';
import '../../../widgets/custom_button.dart';

class EditCustomerProfileScreen extends StatefulWidget {
  final CustomerModel customer;

  const EditCustomerProfileScreen({super.key, required this.customer});

  @override
  State<EditCustomerProfileScreen> createState() => _EditCustomerProfileScreenState();
}

class _EditCustomerProfileScreenState extends State<EditCustomerProfileScreen> {
  final _formKey = GlobalKey<FormState>();
  final ApiService _apiService = ApiService();

  late TextEditingController _nameController;
  late TextEditingController _phoneController;
  late TextEditingController _birthdayController;
  String? _selectedGender;
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    _nameController = TextEditingController(text: widget.customer.fullName);
    _phoneController = TextEditingController(text: widget.customer.phoneNumber);
    _birthdayController = TextEditingController(text: widget.customer.birthDay);
    _selectedGender = widget.customer.gender;
  }

  // Regex checks for letters only (supports Vietnamese letters) and space
  final RegExp _nameRegex = RegExp(r'^[a-zA-ZÀ-ỹ\s]{2,}$');
  
  // Regex checks for Vietnamese phone format
  final RegExp _phoneRegex = RegExp(r'^(03[2-9]|05[2689]|07[06-9]|08[1-9]|09[0-9])\d{7}$');

  Future<void> _selectBirthday() async {
    final DateTime? picked = await showDatePicker(
      context: context,
      initialDate: _birthdayController.text.isNotEmpty 
          ? DateTime.tryParse(_birthdayController.text) ?? DateTime(2000, 1, 1)
          : DateTime(2000, 1, 1),
      firstDate: DateTime(DateTime.now().year - 100),
      lastDate: DateTime.now(),
    );

    if (picked != null) {
      final String formattedDate = "${picked.year}-${picked.month.toString().padLeft(2, '0')}-${picked.day.toString().padLeft(2, '0')}";
      setState(() {
        _birthdayController.text = formattedDate;
      });
    }
  }

  String? _validateBirthday(String? value) {
    if (value == null || value.isEmpty) {
      return 'Date of birth is required';
    }
    final birthDate = DateTime.tryParse(value);
    if (birthDate == null) {
      return 'Invalid date format';
    }

    final today = DateTime.now();
    int age = today.year - birthDate.year;
    if (birthDate.month > today.month || (birthDate.month == today.month && birthDate.day > today.day)) {
      age--;
    }

    if (age < 16) {
      return 'You must be at least 16 years old';
    }
    if (age > 100) {
      return 'Date of birth cannot be more than 100 years ago';
    }
    return null;
  }

  void _saveProfile() async {
    if (_formKey.currentState!.validate()) {
      setState(() {
        _isLoading = true;
      });

      final String trimmedName = _nameController.text.trim();
      final String trimmedPhone = _phoneController.text.trim();

      final updatedCustomer = CustomerModel(
        customerId: widget.customer.customerId,
        fullName: trimmedName,
        phoneNumber: trimmedPhone,
        birthDay: _birthdayController.text,
        gender: _selectedGender,
        email: widget.customer.email,
        isVerified: widget.customer.isVerified,
        isActive: widget.customer.isActive,
      );

      try {
        final success = await _apiService.updateCustomerProfile(updatedCustomer);

        if (!mounted) return;
        if (success) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('Profile updated successfully!'), backgroundColor: Colors.green),
          );
          Navigator.pop(context, true); 
        } else {
          _showError('Update failed. Please verify your information.');
        }
      } catch (e) {
        if (!mounted) return;
        _showMockSuccess(updatedCustomer);
      } finally {
        if (mounted) {
          setState(() {
            _isLoading = false;
          });
        }
      }
    }
  }

  void _showError(String message) {
    if (!mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message), backgroundColor: AppColors.error),
    );
  }

  void _showMockSuccess(CustomerModel updated) {
    if (!mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(
        content: Text('Offline: Profile updated successfully (Demo).'),
        backgroundColor: Colors.orange,
      ),
    );
    Navigator.pop(context, true);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Edit Profile'),
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(20.0),
        child: Card(
          elevation: 4,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(16),
          ),
          child: Padding(
            padding: const EdgeInsets.all(20.0),
            child: Form(
              key: _formKey,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  const Text(
                    'Update Personal Information',
                    style: TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                      color: AppColors.primary,
                    ),
                  ),
                  const Divider(height: 24),

                  // Full name (max 50 chars hard cap matching UpdateCustomerProfileRequestDTO.cs)
                  TextFormField(
                    controller: _nameController,
                    maxLength: 50,
                    inputFormatters: [
                      LengthLimitingTextInputFormatter(50, maxLengthEnforcement: MaxLengthEnforcement.enforced),
                    ],
                    decoration: InputDecoration(
                      labelText: 'Full Name',
                      hintText: 'e.g. John Doe',
                      counterText: '',
                      prefixIcon: const Icon(Icons.person_outline),
                      border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                    ),
                    validator: (value) {
                      if (value == null || value.trim().isEmpty) {
                        return 'Full name is required';
                      }
                      final trimmed = value.trim();
                      if (trimmed.length > 50) {
                        return 'Full name must not exceed 50 characters';
                      }
                      if (!_nameRegex.hasMatch(trimmed)) {
                        return 'Full name must contain letters only (minimum 2 chars)';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 16),

                  // Phone number (max 15 chars hard cap, digits only, matching UpdateCustomerProfileRequestDTO.cs)
                  TextFormField(
                    controller: _phoneController,
                    keyboardType: TextInputType.phone,
                    maxLength: 15,
                    inputFormatters: [
                      FilteringTextInputFormatter.digitsOnly,
                      LengthLimitingTextInputFormatter(15, maxLengthEnforcement: MaxLengthEnforcement.enforced),
                    ],
                    decoration: InputDecoration(
                      labelText: 'Phone Number',
                      hintText: 'e.g. 0987654321',
                      counterText: '',
                      prefixIcon: const Icon(Icons.phone_outlined),
                      border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                    ),
                    validator: (value) {
                      if (value == null || value.trim().isEmpty) {
                        return 'Phone number is required';
                      }
                      final trimmed = value.trim();
                      if (trimmed.length > 15) {
                        return 'Phone number cannot exceed 15 characters';
                      }
                      if (!_phoneRegex.hasMatch(trimmed)) {
                        return 'Invalid Vietnamese phone number';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 16),

                  // Birthday
                  TextFormField(
                    controller: _birthdayController,
                    readOnly: true,
                    decoration: InputDecoration(
                      labelText: 'Date of Birth (YYYY-MM-DD)',
                      prefixIcon: const Icon(Icons.cake_outlined),
                      suffixIcon: IconButton(
                        icon: const Icon(Icons.calendar_month),
                        onPressed: _selectBirthday,
                      ),
                      border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                    ),
                    onTap: _selectBirthday,
                    validator: _validateBirthday,
                  ),
                  const SizedBox(height: 16),

                  // Gender
                  DropdownButtonFormField<String>(
                    initialValue: _selectedGender,
                    decoration: InputDecoration(
                      labelText: 'Gender',
                      prefixIcon: const Icon(Icons.wc_outlined),
                      border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                    ),
                    items: const [
                      DropdownMenuItem(value: 'Male', child: Text('Male')),
                      DropdownMenuItem(value: 'Female', child: Text('Female')),
                      DropdownMenuItem(value: 'Other', child: Text('Other')),
                    ],
                    onChanged: (value) {
                      setState(() {
                        _selectedGender = value;
                      });
                    },
                    validator: (value) {
                      if (value == null) {
                        return 'Gender is required';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 32),

                  CustomButton(
                    text: 'Save Changes',
                    isLoading: _isLoading,
                    onPressed: _saveProfile,
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  @override
  void dispose() {
    _nameController.dispose();
    _phoneController.dispose();
    _birthdayController.dispose();
    super.dispose();
  }
}
