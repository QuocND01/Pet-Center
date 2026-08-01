import 'package:flutter/material.dart';
import 'package:google_sign_in/google_sign_in.dart';
import '../../../constants/app_colors.dart';
import '../../../widgets/custom_button.dart';
import '../../../services/api_service.dart';
import '../../../services/auth_service.dart';
import 'otp_screen.dart';
import 'forgot_password_screen.dart';

class LoginScreen extends StatefulWidget {
  final String? prefilledEmail;
  final String? prefilledPassword;

  const LoginScreen({
    super.key,
    this.prefilledEmail,
    this.prefilledPassword,
  });

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final ApiService _apiService = ApiService();
  final AuthService _authService = AuthService();

  bool _isLoading = false;
  bool _isGoogleLoading = false;
  bool _isObscure = true;
  bool _rememberMe = false;

  final GoogleSignIn _googleSignIn = GoogleSignIn(
    serverClientId:
        '205673219686-i049i9ug1nrhik4oh6521fo06t1tllef.apps.googleusercontent.com',
    scopes: ['email', 'profile'],
  );

  final RegExp _emailRegex = RegExp(r'^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$');

  Future<void> _handleGoogleLogin() async {
    setState(() {
      _isGoogleLoading = true;
    });

    try {
      // Disconnect/signOut first to ensure account picker dialog pops up
      await _googleSignIn.signOut();

      final googleUser = await _googleSignIn.signIn();
      if (googleUser == null) {
        setState(() {
          _isGoogleLoading = false;
        });
        return;
      }

      final authCode = googleUser.serverAuthCode;

      Map<String, dynamic> result;
      try {
        result = await _apiService.googleCallback(
          code: authCode ?? '',
          redirectUri: '',
        );
      } catch (e) {
        if (e.toString().contains('redirect_uri_mismatch')) {
          result = await _apiService.googleCallback(
            code: authCode ?? '',
            redirectUri: 'https://localhost:7010/Auth/GoogleCallback',
          );
        } else {
          rethrow;
        }
      }

      if (!mounted) return;
      setState(() {
        _isGoogleLoading = false;
      });

      final isSuccess = result['success'] == true || result['Success'] == true;
      final tokenVal = result['token'] ?? result['Token'];
      final message =
          result['message'] ?? result['Message'] ?? 'Google login successful!';

      if (isSuccess || (tokenVal != null && tokenVal.toString().isNotEmpty)) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(message),
            backgroundColor: AppColors.success,
            duration: const Duration(seconds: 2),
          ),
        );
        Navigator.pushReplacementNamed(context, '/home');
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(message),
            backgroundColor: AppColors.error,
          ),
        );
      }
    } catch (error) {
      if (!mounted) return;
      setState(() {
        _isGoogleLoading = false;
      });
      final String rawMsg = error.toString().replaceAll('Exception: ', '');
      String userFriendlyMessage = rawMsg;
      if (rawMsg.contains('ApiException: 10') ||
          rawMsg.contains('sign_in_failed')) {
        userFriendlyMessage =
            'Google Sign-In configuration error: Android debug SHA-1 key must be added to Google Cloud Console OAuth Clients.';
      }

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(userFriendlyMessage),
          backgroundColor: AppColors.error,
          duration: const Duration(seconds: 5),
        ),
      );
    }
  }

  @override
  void initState() {
    super.initState();
    if (widget.prefilledEmail != null || widget.prefilledPassword != null) {
      if (widget.prefilledEmail != null) {
        _emailController.text = widget.prefilledEmail!;
      }
      if (widget.prefilledPassword != null) {
        _passwordController.text = widget.prefilledPassword!;
      }
    } else {
      _loadSavedCredentials();
    }
  }

  Future<void> _loadSavedCredentials() async {
    final savedData = await _authService.getRememberMeCredentials();
    if (mounted && savedData['remember'] == true) {
      setState(() {
        _rememberMe = true;
        _emailController.text = savedData['email'] ?? '';
        _passwordController.text = savedData['password'] ?? '';
      });
    }
  }

  void _handleLogin() async {
    if (!_formKey.currentState!.validate()) return;

    final email = _emailController.text.trim();
    final password = _passwordController.text;

    setState(() {
      _isLoading = true;
    });

    try {
      final result = await _apiService.customerLogin(email, password);
      if (!mounted) return;

      setState(() {
        _isLoading = false;
      });

      final isSuccess = result['success'] == true || result['Success'] == true;
      final tokenVal = result['token'] ?? result['Token'];
      final errorType = result['errorType'] ?? result['ErrorType'];
      final message = result['message'] ??
          result['Message'] ??
          'Login failed. Please check your credentials.';

      if (isSuccess || (tokenVal != null && tokenVal.toString().isNotEmpty)) {
        // Save remember me status
        await _authService.saveRememberMeCredentials(
            email, password, _rememberMe);

        if (!mounted) return;
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Login successful! Welcome back.'),
            backgroundColor: AppColors.success,
            duration: Duration(seconds: 2),
          ),
        );
        Navigator.pushReplacementNamed(context, '/home');
      } else if (errorType == 'EmailNotVerified') {
        _showUnverifiedAccountDialog(email, message);
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(message),
            backgroundColor: AppColors.error,
          ),
        );
      }
    } catch (error) {
      if (!mounted) return;
      setState(() {
        _isLoading = false;
      });
      final String cleanMsg = error.toString().replaceAll('Exception: ', '');
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(cleanMsg),
          backgroundColor: AppColors.error,
          duration: const Duration(seconds: 4),
        ),
      );
    }
  }

  void _showUnverifiedAccountDialog(String email, String message) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: const Row(
          children: [
            Icon(Icons.mark_email_unread_outlined, color: AppColors.primary),
            SizedBox(width: 8),
            Text('Account Not Verified',
                style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
          ],
        ),
        content: Text(message),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('Cancel', style: TextStyle(color: Colors.grey)),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(
              backgroundColor: AppColors.primary,
              shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(8)),
            ),
            onPressed: () {
              Navigator.pop(ctx);
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (context) => OtpVerificationScreen(
                    email: email,
                    password: _passwordController.text,
                  ),
                ),
              );
            },
            child: const Text('Verify OTP Now',
                style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );
  }

  void _showForgotPasswordDialog() {
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => ForgotPasswordScreen(
          initialEmail: _emailController.text.trim(),
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding:
                const EdgeInsets.symmetric(horizontal: 24.0, vertical: 16.0),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                // Header Logo & Branding
                Container(
                  padding: const EdgeInsets.all(16.0),
                  decoration: BoxDecoration(
                    color: AppColors.primary.withAlpha(25),
                    shape: BoxShape.circle,
                  ),
                  child: const Icon(
                    Icons.pets,
                    size: 64,
                    color: AppColors.primary,
                  ),
                ),
                const SizedBox(height: 12),
                const Text(
                  'PET CENTER',
                  style: TextStyle(
                    fontSize: 26,
                    fontWeight: FontWeight.bold,
                    color: AppColors.primary,
                    letterSpacing: 2,
                  ),
                ),
                const SizedBox(height: 4),
                const Text(
                  'Your Pet\'s Health & Happiness First',
                  style: TextStyle(
                    fontSize: 13,
                    color: AppColors.textSecondary,
                  ),
                ),
                const SizedBox(height: 32),

                // Login Form Card
                Card(
                  elevation: 4,
                  shadowColor: Colors.black12,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: Padding(
                    padding: const EdgeInsets.all(24.0),
                    child: Form(
                      key: _formKey,
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: [
                          const Text(
                            'Sign In',
                            style: TextStyle(
                              fontSize: 20,
                              fontWeight: FontWeight.bold,
                              color: AppColors.textPrimary,
                            ),
                          ),
                          const SizedBox(height: 20),

                          // Email Field
                          TextFormField(
                            controller: _emailController,
                            keyboardType: TextInputType.emailAddress,
                            textInputAction: TextInputAction.next,
                            decoration: InputDecoration(
                              labelText: 'Email Address',
                              hintText: 'example@email.com',
                              prefixIcon: const Icon(Icons.email_outlined,
                                  color: AppColors.primary),
                              border: OutlineInputBorder(
                                borderRadius: BorderRadius.circular(12),
                              ),
                              enabledBorder: OutlineInputBorder(
                                borderRadius: BorderRadius.circular(12),
                                borderSide: const BorderSide(
                                    color: AppColors.inputBorder),
                              ),
                              focusedBorder: OutlineInputBorder(
                                borderRadius: BorderRadius.circular(12),
                                borderSide: const BorderSide(
                                    color: AppColors.primary, width: 2),
                              ),
                              filled: true,
                              fillColor: Colors.white,
                            ),
                            validator: (value) {
                              if (value == null || value.trim().isEmpty) {
                                return 'Please enter your email address';
                              }
                              if (!_emailRegex.hasMatch(value.trim())) {
                                return 'Please enter a valid email address';
                              }
                              return null;
                            },
                          ),
                          const SizedBox(height: 16),

                          // Password Field
                          TextFormField(
                            controller: _passwordController,
                            obscureText: _isObscure,
                            textInputAction: TextInputAction.done,
                            onFieldSubmitted: (_) => _handleLogin(),
                            decoration: InputDecoration(
                              labelText: 'Password',
                              prefixIcon: const Icon(Icons.lock_outline,
                                  color: AppColors.primary),
                              suffixIcon: IconButton(
                                icon: Icon(
                                  _isObscure
                                      ? Icons.visibility_outlined
                                      : Icons.visibility_off_outlined,
                                  color: AppColors.textSecondary,
                                ),
                                onPressed: () {
                                  setState(() {
                                    _isObscure = !_isObscure;
                                  });
                                },
                              ),
                              border: OutlineInputBorder(
                                borderRadius: BorderRadius.circular(12),
                              ),
                              enabledBorder: OutlineInputBorder(
                                borderRadius: BorderRadius.circular(12),
                                borderSide: const BorderSide(
                                    color: AppColors.inputBorder),
                              ),
                              focusedBorder: OutlineInputBorder(
                                borderRadius: BorderRadius.circular(12),
                                borderSide: const BorderSide(
                                    color: AppColors.primary, width: 2),
                              ),
                              filled: true,
                              fillColor: Colors.white,
                            ),
                            validator: (value) {
                              if (value == null || value.isEmpty) {
                                return 'Please enter your password';
                              }
                              if (value.length < 6) {
                                return 'Password must be at least 6 characters';
                              }
                              return null;
                            },
                          ),
                          const SizedBox(height: 12),

                          // Remember Me & Forgot Password Row
                          Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              Row(
                                children: [
                                  SizedBox(
                                    height: 24,
                                    width: 24,
                                    child: Checkbox(
                                      value: _rememberMe,
                                      activeColor: AppColors.primary,
                                      shape: RoundedRectangleBorder(
                                        borderRadius: BorderRadius.circular(4),
                                      ),
                                      onChanged: (val) {
                                        setState(() {
                                          _rememberMe = val ?? false;
                                        });
                                      },
                                    ),
                                  ),
                                  const SizedBox(width: 8),
                                  const Text(
                                    'Remember me',
                                    style: TextStyle(
                                      fontSize: 13,
                                      color: AppColors.textSecondary,
                                    ),
                                  ),
                                ],
                              ),
                              TextButton(
                                onPressed: _showForgotPasswordDialog,
                                style: TextButton.styleFrom(
                                  padding: EdgeInsets.zero,
                                  minimumSize: Size.zero,
                                  tapTargetSize:
                                      MaterialTapTargetSize.shrinkWrap,
                                ),
                                child: const Text(
                                  'Forgot Password?',
                                  style: TextStyle(
                                    fontSize: 13,
                                    color: AppColors.primary,
                                    fontWeight: FontWeight.w600,
                                  ),
                                ),
                              ),
                            ],
                          ),
                          const SizedBox(height: 24),

                          // Login Button
                          CustomButton(
                            text: 'Login',
                            isLoading: _isLoading,
                            onPressed: _handleLogin,
                          ),
                          const SizedBox(height: 18),

                          const SizedBox(height: 20),

                          // OR Divider (matching website "or continue with")
                          Row(
                            children: [
                              Expanded(
                                  child: Divider(color: Colors.grey.shade300)),
                              const Padding(
                                padding: EdgeInsets.symmetric(horizontal: 12),
                                child: Text(
                                  'or continue with',
                                  style: TextStyle(
                                    fontSize: 13,
                                    color: AppColors.textSecondary,
                                  ),
                                ),
                              ),
                              Expanded(
                                  child: Divider(color: Colors.grey.shade300)),
                            ],
                          ),
                          const SizedBox(height: 20),

                          // Google Sign In Button (matching website button styling)
                          SizedBox(
                            height: 48,
                            child: OutlinedButton(
                              style: OutlinedButton.styleFrom(
                                side: const BorderSide(
                                    color: Color(0xFFDADCE0), width: 1),
                                shape: RoundedRectangleBorder(
                                  borderRadius: BorderRadius.circular(10),
                                ),
                                backgroundColor: Colors.white,
                                elevation: 0,
                              ),
                              onPressed:
                                  _isGoogleLoading ? null : _handleGoogleLogin,
                              child: _isGoogleLoading
                                  ? const SizedBox(
                                      height: 20,
                                      width: 20,
                                      child: CircularProgressIndicator(
                                          strokeWidth: 2,
                                          color: AppColors.primary),
                                    )
                                  : const Row(
                                      mainAxisAlignment:
                                          MainAxisAlignment.center,
                                      children: [
                                        GoogleLogoWidget(size: 20),
                                        SizedBox(width: 12),
                                        Text(
                                          'Continue with Google',
                                          style: TextStyle(
                                            fontSize: 15,
                                            fontWeight: FontWeight.w600,
                                            color: Color(0xFF3C4043),
                                          ),
                                        ),
                                      ],
                                    ),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                ),
                const SizedBox(height: 24),

                // Register Navigation Link
                Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    const Text(
                      "Don't have an account? ",
                      style: TextStyle(color: AppColors.textSecondary),
                    ),
                    GestureDetector(
                      onTap: () {
                        Navigator.pushNamed(context, '/register');
                      },
                      child: const Text(
                        'Register Now',
                        style: TextStyle(
                          color: AppColors.primary,
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
    );
  }

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }
}

class GoogleLogoWidget extends StatelessWidget {
  final double size;

  const GoogleLogoWidget({super.key, this.size = 20});

  @override
  Widget build(BuildContext context) {
    return CustomPaint(
      size: Size(size, size),
      painter: _GoogleLogoPainter(),
    );
  }
}

class _GoogleLogoPainter extends CustomPainter {
  @override
  void paint(Canvas canvas, Size size) {
    final double w = size.width;
    final double h = size.height;

    final paint = Paint()..style = PaintingStyle.fill;

    // Red path (#EA4335)
    paint.color = const Color(0xFFEA4335);
    final pRed = Path()
      ..moveTo(w * 0.5, h * 0.198)
      ..cubicTo(w * 0.574, h * 0.198, w * 0.64, h * 0.223, w * 0.692, h * 0.273)
      ..lineTo(w * 0.835, h * 0.13)
      ..cubicTo(w * 0.748, h * 0.05, w * 0.635, 0, w * 0.5, 0)
      ..cubicTo(w * 0.305, 0, w * 0.136, h * 0.112, w * 0.053, h * 0.275)
      ..lineTo(w * 0.219, h * 0.404)
      ..cubicTo(w * 0.259, h * 0.286, w * 0.37, h * 0.198, w * 0.5, h * 0.198);
    canvas.drawPath(pRed, paint);

    // Blue path (#4285F4)
    paint.color = const Color(0xFF4285F4);
    final pBlue = Path()
      ..moveTo(w * 0.979, h * 0.511)
      ..cubicTo(
          w * 0.979, h * 0.479, w * 0.976, h * 0.447, w * 0.971, h * 0.417)
      ..lineTo(w * 0.5, h * 0.417)
      ..lineTo(w * 0.5, h * 0.605)
      ..lineTo(w * 0.769, h * 0.605)
      ..cubicTo(w * 0.757, h * 0.667, w * 0.722, h * 0.719, w * 0.67, h * 0.754)
      ..lineTo(w * 0.831, h * 0.879)
      ..cubicTo(
          w * 0.925, h * 0.792, w * 0.979, h * 0.663, w * 0.979, h * 0.511);
    canvas.drawPath(pBlue, paint);

    // Yellow path (#FBBC05)
    paint.color = const Color(0xFFFBBC05);
    final pYellow = Path()
      ..moveTo(w * 0.219, h * 0.596)
      ..cubicTo(w * 0.209, h * 0.565, w * 0.204, h * 0.533, w * 0.204, h * 0.5)
      ..cubicTo(
          w * 0.204, h * 0.467, w * 0.209, h * 0.435, w * 0.219, h * 0.404)
      ..lineTo(w * 0.053, h * 0.275)
      ..cubicTo(w * 0.019, h * 0.343, 0, h * 0.419, 0, h * 0.5)
      ..cubicTo(0, h * 0.581, w * 0.019, h * 0.657, w * 0.053, h * 0.725)
      ..lineTo(w * 0.219, h * 0.596);
    canvas.drawPath(pYellow, paint);

    // Green path (#34A853)
    paint.color = const Color(0xFF34A853);
    final pGreen = Path()
      ..moveTo(w * 0.5, h * 1.0)
      ..cubicTo(w * 0.635, h * 1.0, w * 0.749, h * 0.956, w * 0.831, h * 0.879)
      ..lineTo(w * 0.67, h * 0.754)
      ..cubicTo(w * 0.625, h * 0.785, w * 0.567, h * 0.802, w * 0.5, h * 0.802)
      ..cubicTo(w * 0.37, h * 0.802, w * 0.259, h * 0.714, w * 0.219, h * 0.596)
      ..lineTo(w * 0.053, h * 0.725)
      ..cubicTo(w * 0.136, h * 0.888, w * 0.305, h * 1.0, w * 0.5, h * 1.0);
    canvas.drawPath(pGreen, paint);
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
}
