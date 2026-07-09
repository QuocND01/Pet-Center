import 'package:flutter/material.dart';
import 'dart:async';
import '../../../constants/app_colors.dart';
import '../../../widgets/custom_button.dart';
import '../../../services/api_service.dart';

class OtpVerificationScreen extends StatefulWidget {
  final String email;

  const OtpVerificationScreen({super.key, required this.email});

  @override
  State<OtpVerificationScreen> createState() => _OtpVerificationScreenState();
}

class _OtpVerificationScreenState extends State<OtpVerificationScreen> {
  final _formKey = GlobalKey<FormState>();
  final _otpController = TextEditingController();
  final ApiService _apiService = ApiService();
  bool _isLoading = false;

  int _resendCooldown = 60; // 60-second cooldown
  Timer? _cooldownTimer;

  @override
  void initState() {
    super.initState();
    _startCooldown();
  }

  void _startCooldown() {
    setState(() {
      _resendCooldown = 60;
    });
    _cooldownTimer?.cancel();
    _cooldownTimer = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (_resendCooldown > 0) {
        setState(() {
          _resendCooldown--;
        });
      } else {
        _cooldownTimer?.cancel();
      }
    });
  }

  void _handleVerifyOtp() {
    if (_formKey.currentState!.validate()) {
      setState(() {
        _isLoading = true;
      });

      _apiService.verifyOtp(widget.email, _otpController.text.trim()).then((result) {
        if (!mounted) return;
        setState(() {
          _isLoading = false;
        });

        final isSuccess = result['success'] == true || result['Success'] == true;

        if (isSuccess) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('Account verified successfully! You can login now.'),
              backgroundColor: Colors.green,
            ),
          );
          Navigator.popUntil(context, ModalRoute.withName('/login'));
        } else {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text(result['message'] ?? result['Message'] ?? 'Incorrect or expired verification code.'),
              backgroundColor: AppColors.error,
            ),
          );
        }
      }).catchError((error) {
        if (!mounted) return;
        setState(() {
          _isLoading = false;
        });
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('API Connection Error: $error'),
            backgroundColor: AppColors.error,
          ),
        );
      });
    }
  }

  void _handleResendOtp() {
    if (_resendCooldown > 0) return;

    setState(() {
      _isLoading = true;
    });

    _apiService.resendOtp(widget.email).then((result) {
      if (!mounted) return;
      setState(() {
        _isLoading = false;
      });

      final isSuccess = result['success'] == true || result['Success'] == true;

      if (isSuccess) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('A new OTP has been sent. Please check your email.'),
            backgroundColor: Colors.green,
          ),
        );
        _startCooldown();
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(result['message'] ?? result['Message'] ?? 'Failed to resend OTP.'),
            backgroundColor: AppColors.error,
          ),
        );
      }
    }).catchError((error) {
      if (!mounted) return;
      setState(() {
        _isLoading = false;
      });
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('API Connection Error: $error'),
          backgroundColor: AppColors.error,
        ),
      );
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Verify OTP'),
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
      ),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24.0),
          child: Center(
            child: SingleChildScrollView(
              child: Card(
                elevation: 4,
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                child: Padding(
                  padding: const EdgeInsets.all(20.0),
                  child: Form(
                    key: _formKey,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        const Icon(
                          Icons.mark_email_unread_outlined,
                          size: 64,
                          color: AppColors.primary,
                        ),
                        const SizedBox(height: 16),
                        const Text(
                          'Verify Your Account',
                          textAlign: TextAlign.center,
                          style: TextStyle(
                            fontSize: 20,
                            fontWeight: FontWeight.bold,
                            color: AppColors.primary,
                          ),
                        ),
                        const SizedBox(height: 8),
                        Text(
                          'We have sent a verification code to:\n${widget.email}',
                          textAlign: TextAlign.center,
                          style: const TextStyle(
                            fontSize: 14,
                            color: AppColors.textSecondary,
                          ),
                        ),
                        const SizedBox(height: 32),

                        // OTP input
                        TextFormField(
                          controller: _otpController,
                          keyboardType: TextInputType.number,
                          textAlign: TextAlign.center,
                          style: const TextStyle(fontSize: 22, fontWeight: FontWeight.bold, letterSpacing: 8),
                          decoration: InputDecoration(
                            hintText: '000000',
                            hintStyle: TextStyle(color: Colors.grey.shade400, letterSpacing: 8),
                            border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                          ),
                          validator: (value) {
                            if (value == null || value.isEmpty) {
                              return 'Please enter the OTP code';
                            }
                            if (value.length < 4) {
                              return 'Invalid OTP code';
                            }
                            return null;
                          },
                        ),
                        const SizedBox(height: 24),

                        CustomButton(
                          text: 'Verify',
                          isLoading: _isLoading,
                          onPressed: _handleVerifyOtp,
                        ),
                        const SizedBox(height: 16),

                        // Resend OTP
                        Row(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            const Text("Didn't receive the code? "),
                            TextButton(
                              onPressed: _resendCooldown == 0 ? _handleResendOtp : null,
                              child: Text(
                                _resendCooldown == 0
                                    ? 'Resend now'
                                    : 'Resend in (${_resendCooldown}s)',
                                style: TextStyle(
                                  color: _resendCooldown == 0 ? AppColors.primary : Colors.grey,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }

  @override
  void dispose() {
    _otpController.dispose();
    _cooldownTimer?.cancel();
    super.dispose();
  }
}
