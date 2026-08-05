import 'package:flutter/material.dart';
import '../../../constants/app_colors.dart';
import '../../../services/api_service.dart';
import '../../../utils/app_error_utils.dart';

class ChangePasswordScreen extends StatefulWidget {
  const ChangePasswordScreen({super.key});

  @override
  State<ChangePasswordScreen> createState() => _ChangePasswordScreenState();
}

class _ChangePasswordScreenState extends State<ChangePasswordScreen> {
  final ApiService _apiService = ApiService();

  final TextEditingController _currentPasswordController = TextEditingController();
  final TextEditingController _newPasswordController = TextEditingController();
  final TextEditingController _confirmPasswordController = TextEditingController();

  bool _obscureCurrent = true;
  bool _obscureNew = true;
  bool _obscureConfirm = true;

  bool _isLoading = false;
  String? _apiError;
  String? _apiSuccess;

  // Field-level error messages directly under inputs
  String? _currentPasswordError;
  String? _newPasswordError;
  String? _confirmPasswordError;

  // Real-time password text for criteria & strength
  String _newPasswordText = '';

  @override
  void initState() {
    super.initState();

    _currentPasswordController.addListener(() {
      if (_currentPasswordError != null && _currentPasswordController.text.trim().isNotEmpty) {
        setState(() {
          _currentPasswordError = null;
        });
      }
    });

    _newPasswordController.addListener(() {
      setState(() {
        _newPasswordText = _newPasswordController.text;
        if (_newPasswordError != null && _isNewPasswordValid) {
          _newPasswordError = null;
        }
      });
    });

    _confirmPasswordController.addListener(() {
      if (_confirmPasswordError != null && _confirmPasswordController.text.trim().isNotEmpty) {
        setState(() {
          _confirmPasswordError = null;
        });
      }
    });
  }

  @override
  void dispose() {
    _currentPasswordController.dispose();
    _newPasswordController.dispose();
    _confirmPasswordController.dispose();
    super.dispose();
  }

  // Password validation checks
  bool get _hasMinLength => _newPasswordText.trim().length >= 6 && _newPasswordText.trim().length <= 50;
  bool get _startsWithUppercase {
    final trimmed = _newPasswordText.trim();
    return trimmed.isNotEmpty && RegExp(r'^[A-Z]').hasMatch(trimmed);
  }
  bool get _hasAtSymbol => _newPasswordText.trim().contains('@');
  bool get _hasDigit => RegExp(r'[0-9]').hasMatch(_newPasswordText.trim());
  bool get _hasNoSpaces => _newPasswordText.isNotEmpty && !RegExp(r'\s').hasMatch(_newPasswordText);

  int get _satisfiedCount {
    int count = 0;
    if (_hasMinLength) count++;
    if (_startsWithUppercase) count++;
    if (_hasAtSymbol) count++;
    if (_hasDigit) count++;
    if (_hasNoSpaces) count++;
    return count;
  }

  bool get _isNewPasswordValid => _satisfiedCount == 5;

  Color get _strengthColor {
    if (_newPasswordText.isEmpty) return Colors.grey.shade300;
    if (_satisfiedCount <= 2) return Colors.red;
    if (_satisfiedCount <= 4) return Colors.orange;
    return AppColors.primary;
  }

  String get _strengthText {
    if (_newPasswordText.isEmpty) return '';
    if (_satisfiedCount <= 2) return 'Weak';
    if (_satisfiedCount <= 4) return 'Medium';
    return 'Strong';
  }

  void _handleChangePassword() async {
    FocusScope.of(context).unfocus();
    setState(() {
      _apiError = null;
      _apiSuccess = null;
      _currentPasswordError = null;
      _newPasswordError = null;
      _confirmPasswordError = null;
    });

    final currentPass = _currentPasswordController.text.trim();
    final newPass = _newPasswordController.text.trim();
    final confirmPass = _confirmPasswordController.text.trim();

    bool hasError = false;

    // Field 1 validation
    if (currentPass.isEmpty) {
      _currentPasswordError = 'Please enter your current password.';
      hasError = true;
    }

    // Field 2 validation
    if (newPass.isEmpty) {
      _newPasswordError = 'Please enter a new password.';
      hasError = true;
    } else if (!_isNewPasswordValid) {
      _newPasswordError = 'New password does not meet format requirements below.';
      hasError = true;
    }

    // Field 3 validation
    if (confirmPass.isEmpty) {
      _confirmPasswordError = 'Please confirm your new password.';
      hasError = true;
    } else if (newPass != confirmPass) {
      _confirmPasswordError = 'Confirm password does not match new password.';
      hasError = true;
    }

    if (hasError) {
      setState(() {});
      return;
    }

    setState(() {
      _isLoading = true;
    });

    try {
      final res = await _apiService.changePassword(
        currentPassword: currentPass,
        newPassword: newPass,
        confirmNewPassword: confirmPass,
      );

      if (mounted) {
        final isSuccess = res['success'] == true || res['Success'] == true;
        final msg = res['message'] ?? res['Message'] ?? (isSuccess ? 'Password changed successfully!' : 'Failed to change password.');

        if (isSuccess) {
          setState(() {
            _apiSuccess = msg;
            _currentPasswordController.clear();
            _newPasswordController.clear();
            _confirmPasswordController.clear();
          });

          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text(msg),
              backgroundColor: Colors.green,
            ),
          );

          Future.delayed(const Duration(seconds: 2), () {
            if (mounted) {
              Navigator.pop(context, true);
            }
          });
        } else {
          if (msg.toString().toLowerCase().contains('current password')) {
            setState(() {
              _currentPasswordError = msg;
            });
          } else {
            setState(() {
              _apiError = msg;
            });
          }
        }
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _apiError = AppErrorUtils.getFriendlyMessage(e);
        });
      }
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
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text(
          'Change Password',
          style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18),
        ),
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
        elevation: 0,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(20.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header Security Banner Card
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: AppColors.primary.withOpacity(0.08),
                borderRadius: BorderRadius.circular(16),
                border: Border.all(color: AppColors.primary.withOpacity(0.2)),
              ),
              child: Row(
                children: [
                  Container(
                    padding: const EdgeInsets.all(10),
                    decoration: const BoxDecoration(
                      color: AppColors.primary,
                      shape: BoxShape.circle,
                    ),
                    child: const Icon(Icons.shield_outlined, color: Colors.white, size: 28),
                  ),
                  const SizedBox(width: 14),
                  const Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Account Security',
                          style: TextStyle(
                            fontWeight: FontWeight.bold,
                            fontSize: 16,
                            color: AppColors.textPrimary,
                          ),
                        ),
                        SizedBox(height: 4),
                        Text(
                          'Please enter your current password and create a strong new password.',
                          style: TextStyle(fontSize: 12, color: AppColors.textSecondary, height: 1.4),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 20),

            // Top API Success Alert
            if (_apiSuccess != null)
              Container(
                margin: const EdgeInsets.only(bottom: 16),
                padding: const EdgeInsets.all(14),
                decoration: BoxDecoration(
                  color: Colors.green.shade50,
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(color: Colors.green.shade300),
                ),
                child: Row(
                  children: [
                    const Icon(Icons.check_circle, color: Colors.green, size: 20),
                    const SizedBox(width: 10),
                    Expanded(
                      child: Text(
                        _apiSuccess!,
                        style: TextStyle(color: Colors.green.shade900, fontWeight: FontWeight.bold, fontSize: 13),
                      ),
                    ),
                  ],
                ),
              ),

            // Top API Error Alert
            if (_apiError != null)
              Container(
                margin: const EdgeInsets.only(bottom: 16),
                padding: const EdgeInsets.all(14),
                decoration: BoxDecoration(
                  color: Colors.red.shade50,
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(color: Colors.red.shade300),
                ),
                child: Row(
                  children: [
                    const Icon(Icons.error_outline, color: Colors.red, size: 20),
                    const SizedBox(width: 10),
                    Expanded(
                      child: Text(
                        _apiError!,
                        style: TextStyle(color: Colors.red.shade900, fontWeight: FontWeight.bold, fontSize: 13),
                      ),
                    ),
                  ],
                ),
              ),

            // ==========================================
            // FIELD 1: Current Password
            // ==========================================
            _buildPasswordField(
              controller: _currentPasswordController,
              label: 'Current Password',
              hint: 'Enter current password',
              obscureText: _obscureCurrent,
              errorText: _currentPasswordError,
              onToggleObscure: () {
                setState(() {
                  _obscureCurrent = !_obscureCurrent;
                });
              },
            ),
            const SizedBox(height: 20),

            // ==========================================
            // FIELD 2: New Password & Live Requirements Card
            // ==========================================
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(16),
                border: Border.all(
                  color: _newPasswordError != null ? Colors.red.shade400 : Colors.grey.shade200,
                  width: _newPasswordError != null ? 1.5 : 1.0,
                ),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withOpacity(0.03),
                    blurRadius: 10,
                    offset: const Offset(0, 4),
                  ),
                ],
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  _buildPasswordField(
                    controller: _newPasswordController,
                    label: 'New Password',
                    hint: 'Enter new password',
                    obscureText: _obscureNew,
                    errorText: _newPasswordError,
                    onToggleObscure: () {
                      setState(() {
                        _obscureNew = !_obscureNew;
                      });
                    },
                    insideCard: true,
                  ),

                  // Password Strength Bar
                  if (_newPasswordText.isNotEmpty) ...[
                    const SizedBox(height: 12),
                    Row(
                      children: [
                        Expanded(
                          child: ClipRRect(
                            borderRadius: BorderRadius.circular(4),
                            child: LinearProgressIndicator(
                              value: _satisfiedCount / 5.0,
                              minHeight: 6,
                              backgroundColor: Colors.grey.shade200,
                              valueColor: AlwaysStoppedAnimation<Color>(_strengthColor),
                            ),
                          ),
                        ),
                        const SizedBox(width: 12),
                        Text(
                          _strengthText,
                          style: TextStyle(
                            fontSize: 12,
                            fontWeight: FontWeight.bold,
                            color: _strengthColor,
                          ),
                        ),
                      ],
                    ),
                  ],

                  const SizedBox(height: 14),
                  const Divider(height: 1),
                  const SizedBox(height: 12),

                  // Live Requirement Checklist
                  const Text(
                    'New password requirements:',
                    style: TextStyle(
                      fontSize: 13,
                      fontWeight: FontWeight.bold,
                      color: AppColors.textPrimary,
                    ),
                  ),
                  const SizedBox(height: 8),
                  _buildCheckItem('6 to 50 characters long', _hasMinLength),
                  _buildCheckItem('First character must be UPPERCASE (A-Z)', _startsWithUppercase),
                  _buildCheckItem('Must contain symbol @', _hasAtSymbol),
                  _buildCheckItem('Must contain at least 1 digit (0-9)', _hasDigit),
                  _buildCheckItem('No spaces allowed', _hasNoSpaces),
                ],
              ),
            ),
            const SizedBox(height: 20),

            // ==========================================
            // FIELD 3: Confirm New Password
            // ==========================================
            _buildPasswordField(
              controller: _confirmPasswordController,
              label: 'Confirm New Password',
              hint: 'Re-enter new password',
              obscureText: _obscureConfirm,
              errorText: _confirmPasswordError,
              onToggleObscure: () {
                setState(() {
                  _obscureConfirm = !_obscureConfirm;
                });
              },
            ),
            const SizedBox(height: 28),

            // Submit Button
            SizedBox(
              width: double.infinity,
              height: 52,
              child: ElevatedButton(
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.primary,
                  foregroundColor: Colors.white,
                  elevation: 2,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
                onPressed: _isLoading ? null : _handleChangePassword,
                child: _isLoading
                    ? const SizedBox(
                        width: 24,
                        height: 24,
                        child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2.5),
                      )
                    : const Text(
                        'Change Password',
                        style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                      ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildPasswordField({
    required TextEditingController controller,
    required String label,
    required String hint,
    required bool obscureText,
    required VoidCallback onToggleObscure,
    String? errorText,
    bool insideCard = false,
  }) {
    final hasError = errorText != null;

    final fieldWidget = Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          label,
          style: const TextStyle(
            fontSize: 14,
            fontWeight: FontWeight.bold,
            color: AppColors.textPrimary,
          ),
        ),
        const SizedBox(height: 6),
        TextField(
          controller: controller,
          obscureText: obscureText,
          maxLength: 50,
          decoration: InputDecoration(
            hintText: hint,
            hintStyle: const TextStyle(color: Colors.grey, fontSize: 14),
            counterText: '', // Hide character counter text
            prefixIcon: Icon(
              Icons.lock_outline,
              color: hasError ? Colors.red : AppColors.primary,
              size: 20,
            ),
            suffixIcon: IconButton(
              icon: Icon(
                obscureText ? Icons.visibility_off_outlined : Icons.visibility_outlined,
                color: Colors.grey,
                size: 20,
              ),
              onPressed: onToggleObscure,
            ),
            filled: true,
            fillColor: insideCard ? AppColors.background : Colors.white,
            contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
            border: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: BorderSide(color: hasError ? Colors.red : Colors.grey.shade300),
            ),
            enabledBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: BorderSide(color: hasError ? Colors.red : Colors.grey.shade300),
            ),
            focusedBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: BorderSide(
                color: hasError ? Colors.red : AppColors.primary,
                width: 2,
              ),
            ),
          ),
        ),
        // Direct Field Error Message Log Below Input Box
        if (hasError)
          Padding(
            padding: const EdgeInsets.only(top: 6, left: 4),
            child: Row(
              children: [
                const Icon(Icons.error_outline, color: Colors.red, size: 14),
                const SizedBox(width: 6),
                Expanded(
                  child: Text(
                    errorText,
                    style: const TextStyle(
                      color: Colors.red,
                      fontSize: 12,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ),
              ],
            ),
          ),
      ],
    );

    return fieldWidget;
  }

  Widget _buildCheckItem(String text, bool isSatisfied) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 3.0),
      child: Row(
        children: [
          AnimatedContainer(
            duration: const Duration(milliseconds: 200),
            width: 18,
            height: 18,
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              color: isSatisfied ? AppColors.primary : Colors.grey.shade200,
            ),
            child: Icon(
              isSatisfied ? Icons.check : Icons.circle_outlined,
              color: isSatisfied ? Colors.white : Colors.grey.shade400,
              size: 12,
            ),
          ),
          const SizedBox(width: 10),
          Expanded(
            child: Text(
              text,
              style: TextStyle(
                fontSize: 12,
                color: isSatisfied ? AppColors.primary : AppColors.textSecondary,
                fontWeight: isSatisfied ? FontWeight.bold : FontWeight.normal,
              ),
            ),
          ),
        ],
      ),
    );
  }
}
