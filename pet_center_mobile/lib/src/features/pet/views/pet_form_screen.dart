import 'dart:io';
import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import '../../../constants/app_colors.dart';
import '../../../models/pet_model.dart';
import '../../../services/api_service.dart';

class PetFormScreen extends StatefulWidget {
  final PetModel? pet;

  const PetFormScreen({super.key, this.pet});

  @override
  State<PetFormScreen> createState() => _PetFormScreenState();
}

class _PetFormScreenState extends State<PetFormScreen> {
  final _formKey = GlobalKey<FormState>();
  final ApiService _apiService = ApiService();
  final ImagePicker _picker = ImagePicker();

  late TextEditingController _nameController;
  late TextEditingController _speciesController;
  late TextEditingController _breedController;
  late TextEditingController _weightController;
  late TextEditingController _dobController;
  late TextEditingController _noteController;

  String _gender = 'Male';
  File? _selectedImage;
  bool _isLoading = false;

  // Chuẩn hóa theo Client Web: Chỉ có Dog và Cat
  final List<String> _speciesOptions = ['Dog', 'Cat'];
  final List<String> _genderOptions = ['Male', 'Female'];

  @override
  void initState() {
    super.initState();
    _nameController = TextEditingController(text: widget.pet?.petName ?? '');

    // Đảm bảo loài (Species) mặc định là Dog nếu không khớp
    String initialSpecies = widget.pet?.species ?? 'Dog';
    if (!_speciesOptions.contains(initialSpecies)) {
      initialSpecies = 'Dog';
    }
    _speciesController = TextEditingController(text: initialSpecies);

    _breedController = TextEditingController(text: widget.pet?.breed ?? '');
    _weightController = TextEditingController(
        text: widget.pet?.weight != null ? widget.pet!.weight.toString() : '');
    _dobController = TextEditingController(text: widget.pet?.dateOfBirth ?? '');
    _noteController = TextEditingController(text: widget.pet?.note ?? '');

    // Chuẩn hóa Gender
    if (widget.pet != null && widget.pet!.gender.isNotEmpty) {
      if (widget.pet!.gender.toLowerCase() == 'female') {
        _gender = 'Female';
      } else {
        _gender = 'Male';
      }
    }
  }

  @override
  void dispose() {
    _nameController.dispose();
    _speciesController.dispose();
    _breedController.dispose();
    _weightController.dispose();
    _dobController.dispose();
    _noteController.dispose();
    super.dispose();
  }

  Future<void> _pickImage() async {
    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(16)),
      ),
      builder: (ctx) => SafeArea(
        child: Wrap(
          children: [
            ListTile(
              leading: const Icon(Icons.photo_library_outlined,
                  color: AppColors.primary),
              title: const Text('Choose from Library'),
              onTap: () async {
                Navigator.pop(ctx);
                final picked =
                    await _picker.pickImage(source: ImageSource.gallery);
                if (picked != null) {
                  setState(() {
                    _selectedImage = File(picked.path);
                  });
                }
              },
            ),
            ListTile(
              leading: const Icon(Icons.camera_alt_outlined,
                  color: AppColors.primary),
              title: const Text('Take a Photo'),
              onTap: () async {
                Navigator.pop(ctx);
                final picked =
                    await _picker.pickImage(source: ImageSource.camera);
                if (picked != null) {
                  setState(() {
                    _selectedImage = File(picked.path);
                  });
                }
              },
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _selectDateOfBirth() async {
    DateTime initial = DateTime.now();
    if (_dobController.text.isNotEmpty) {
      try {
        initial = DateTime.parse(_dobController.text);
      } catch (_) {}
    }

    final picked = await showDatePicker(
      context: context,
      initialDate: initial,
      firstDate: DateTime(2000),
      lastDate: DateTime.now(), // Không cho phép chọn ngày tương lai
      builder: (context, child) {
        return Theme(
          data: Theme.of(context).copyWith(
            colorScheme: const ColorScheme.light(primary: AppColors.primary),
          ),
          child: child!,
        );
      },
    );

    if (picked != null) {
      setState(() {
        final year = picked.year.toString().padLeft(4, '0');
        final month = picked.month.toString().padLeft(2, '0');
        final day = picked.day.toString().padLeft(2, '0');
        _dobController.text = '$year-$month-$day';
      });
    }
  }

  Future<void> _submitForm() async {
    if (!_formKey.currentState!.validate()) return;

    // Validate DOB (Double check không cho ngày tương lai)
    if (_dobController.text.isNotEmpty) {
      final dob = DateTime.tryParse(_dobController.text);
      if (dob != null && dob.isAfter(DateTime.now())) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Date of birth cannot be in the future.'),
            backgroundColor: AppColors.error,
          ),
        );
        return;
      }
    }

    setState(() {
      _isLoading = true;
    });

    try {
      final name = _nameController.text.trim();
      final species = _speciesController.text.trim();
      final breed = _breedController.text.trim();
      final weight = double.tryParse(_weightController.text.trim());
      final dob = _dobController.text.trim();
      final note = _noteController.text.trim();

      bool success = false;
      if (widget.pet != null) {
        success = await _apiService.updatePet(
          petId: widget.pet!.petId,
          petName: name,
          species: species,
          breed: breed,
          gender: _gender,
          weight: weight,
          note: note,
          dateOfBirth: dob,
          imageFile: _selectedImage,
        );
      } else {
        success = await _apiService.addPet(
          petName: name,
          species: species,
          breed: breed,
          gender: _gender,
          weight: weight,
          note: note,
          dateOfBirth: dob,
          imageFile: _selectedImage,
        );
      }

      if (!mounted) return;

      if (success) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(
              widget.pet != null
                  ? 'Pet profile updated successfully.'
                  : 'New pet added successfully.',
            ),
            backgroundColor: AppColors.success,
          ),
        );
        Navigator.pop(context, true);
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Failed to save information. Please try again.'),
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
    final isEditing = widget.pet != null;
    final avatarUrl = widget.pet?.getFullAvatarUrl(ApiService.baseUrl);

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Text(isEditing ? 'Edit Pet Profile' : 'Add New Pet'),
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
              // Avatar Selector
              Center(
                child: Stack(
                  children: [
                    Container(
                      width: 110,
                      height: 110,
                      decoration: BoxDecoration(
                        shape: BoxShape.circle,
                        color: AppColors.primary.withOpacity(0.12),
                        border: Border.all(color: AppColors.primary, width: 2),
                      ),
                      child: ClipOval(
                        child: _selectedImage != null
                            ? Image.file(_selectedImage!, fit: BoxFit.cover)
                            : (avatarUrl != null
                                ? Image.network(
                                    avatarUrl,
                                    fit: BoxFit.cover,
                                    errorBuilder: (ctx, err, stack) =>
                                        const Icon(Icons.pets,
                                            size: 50, color: AppColors.primary),
                                  )
                                : const Icon(Icons.pets,
                                    size: 50, color: AppColors.primary)),
                      ),
                    ),
                    Positioned(
                      bottom: 0,
                      right: 0,
                      child: InkWell(
                        onTap: _pickImage,
                        child: Container(
                          padding: const EdgeInsets.all(8),
                          decoration: const BoxDecoration(
                            color: AppColors.primary,
                            shape: BoxShape.circle,
                          ),
                          child: const Icon(Icons.camera_alt,
                              color: Colors.white, size: 20),
                        ),
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 20),

              // Form fields card
              Card(
                shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(16)),
                elevation: 2,
                child: Padding(
                  padding: const EdgeInsets.all(16.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // Pet Name
                      TextFormField(
                        controller: _nameController,
                        textInputAction: TextInputAction.next,
                        decoration: InputDecoration(
                          labelText: 'Pet Name *',
                          hintText: 'e.g., Fluffy, Bobo',
                          prefixIcon: const Icon(Icons.badge_outlined,
                              color: AppColors.primary),
                          border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(10)),
                        ),
                        validator: (val) {
                          if (val == null || val.trim().isEmpty) {
                            return 'Pet name is required.';
                          }
                          if (!RegExp(r"^[\p{L}\p{N}\s'\-]+$", unicode: true)
                              .hasMatch(val)) {
                            return 'Pet name contains invalid characters.';
                          }
                          return null;
                        },
                      ),
                      const SizedBox(height: 16),

                      // Species Dropdown (Chỉ còn Dog & Cat)
                      DropdownButtonFormField<String>(
                        value: _speciesController.text,
                        decoration: InputDecoration(
                          labelText: 'Species *',
                          prefixIcon: const Icon(Icons.category_outlined,
                              color: AppColors.primary),
                          border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(10)),
                        ),
                        items: _speciesOptions.map((s) {
                          return DropdownMenuItem(value: s, child: Text(s));
                        }).toList(),
                        onChanged: (val) {
                          if (val != null) {
                            setState(() {
                              _speciesController.text = val;
                            });
                          }
                        },
                      ),
                      const SizedBox(height: 16),

                      // Breed
                      TextFormField(
                        controller: _breedController,
                        textInputAction: TextInputAction.next,
                        decoration: InputDecoration(
                          labelText: 'Breed *',
                          hintText: 'e.g., Poodle, British Shorthair',
                          prefixIcon: const Icon(Icons.pets_outlined,
                              color: AppColors.primary),
                          border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(10)),
                        ),
                        validator: (val) {
                          if (val == null || val.trim().isEmpty) {
                            return 'Breed is required.';
                          }
                          return null;
                        },
                      ),
                      const SizedBox(height: 16),

                      // Gender Dropdown
                      DropdownButtonFormField<String>(
                        value: _gender,
                        decoration: InputDecoration(
                          labelText: 'Gender *',
                          prefixIcon: const Icon(Icons.transgender_outlined,
                              color: AppColors.primary),
                          border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(10)),
                        ),
                        items: _genderOptions.map((g) {
                          return DropdownMenuItem(value: g, child: Text(g));
                        }).toList(),
                        onChanged: (val) {
                          if (val != null) {
                            setState(() {
                              _gender = val;
                            });
                          }
                        },
                      ),
                      const SizedBox(height: 16),

                      // Weight
                      TextFormField(
                        controller: _weightController,
                        textInputAction: TextInputAction.next,
                        keyboardType: const TextInputType.numberWithOptions(
                            decimal: true),
                        decoration: InputDecoration(
                          labelText: 'Weight (kg)',
                          hintText: 'e.g., 4.5',
                          prefixIcon: const Icon(Icons.scale_outlined,
                              color: AppColors.primary),
                          border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(10)),
                        ),
                        validator: (val) {
                          if (val != null && val.trim().isNotEmpty) {
                            final weight = double.tryParse(val.trim());
                            if (weight == null || weight < 0) {
                              return 'Weight must be a non-negative number.';
                            }
                          }
                          return null;
                        },
                      ),
                      const SizedBox(height: 16),

                      // Date of Birth
                      TextFormField(
                        controller: _dobController,
                        readOnly: true,
                        onTap: _selectDateOfBirth,
                        decoration: InputDecoration(
                          labelText: 'Date of Birth',
                          hintText: 'YYYY-MM-DD',
                          prefixIcon: const Icon(Icons.cake_outlined,
                              color: AppColors.primary),
                          suffixIcon: IconButton(
                            icon: const Icon(Icons.calendar_today_outlined),
                            onPressed: _selectDateOfBirth,
                          ),
                          border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(10)),
                        ),
                      ),
                      const SizedBox(height: 16),

                      // Note
                      TextFormField(
                        controller: _noteController,
                        textInputAction: TextInputAction.done,
                        maxLines: 3,
                        decoration: InputDecoration(
                          labelText: 'Medical Notes (Optional)',
                          hintText: 'Allergies, habits, vaccination...',
                          prefixIcon: const Icon(Icons.notes_outlined,
                              color: AppColors.primary),
                          border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(10)),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
              const SizedBox(height: 24),

              // Save button
              SizedBox(
                width: double.infinity,
                height: 50,
                child: ElevatedButton.icon(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppColors.primary,
                    foregroundColor: Colors.white,
                    shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12)),
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
                        : (isEditing ? 'Update Pet' : 'Save Pet 🐾'),
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
