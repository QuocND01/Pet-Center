import 'dart:async';
import 'dart:io';
import 'package:flutter/material.dart';
import 'src/constants/app_colors.dart';
import 'src/services/api_service.dart';
import 'src/features/auth/views/login_screen.dart';
import 'src/features/home/views/home_screen.dart';
import 'src/features/customer/views/profile_screen.dart';
import 'src/features/auth/views/register_screen.dart';
import 'src/features/services/views/service_list_screen.dart';
import 'src/features/auth/views/forgot_password_screen.dart';
import 'src/features/auth/views/reset_password_screen.dart';
import 'src/features/auth/views/change_password_screen.dart';
import 'src/features/orders/views/order_list_screen.dart';
import 'src/features/address/views/address_list_screen.dart';

import 'package:app_links/app_links.dart';

class MyHttpOverrides extends HttpOverrides {
  @override
  HttpClient createHttpClient(SecurityContext? context) {
    return super.createHttpClient(context)
      ..badCertificateCallback =
          (X509Certificate cert, String host, int port) => true;
  }
}

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  HttpOverrides.global = MyHttpOverrides();
  runApp(const MyApp());
}

class MyApp extends StatefulWidget {
  const MyApp({super.key});

  @override
  State<MyApp> createState() => _MyAppState();
}

class _MyAppState extends State<MyApp> {
  final GlobalKey<NavigatorState> _navigatorKey = GlobalKey<NavigatorState>();
  StreamSubscription<Uri>? _linkSubscription;

  @override
  void initState() {
    super.initState();
    _initDeepLinks();
  }

  @override
  void dispose() {
    _linkSubscription?.cancel();
    super.dispose();
  }

  Future<void> _initDeepLinks() async {
    try {
      final appLinks = AppLinks();

      // Handle link tapped when app is opened from cold start
      final uri = await appLinks.getInitialLink();
      if (uri != null) {
        _handleDeepLink(uri);
      }

      // Handle link tapped while app is running in background/foreground
      _linkSubscription = appLinks.uriLinkStream.listen(
        (uri) {
          _handleDeepLink(uri);
        },
        onError: (err) {
          debugPrint('Deep link stream error: $err');
        },
      );
    } catch (e) {
      debugPrint('Deep link initialization error: $e');
    }
  }

  void _handleDeepLink(Uri uri) {
    final path = uri.path.toLowerCase();
    final host = uri.host.toLowerCase();

    if (host == 'reset-password' ||
        path.contains('/resetpassword') ||
        path.contains('/reset-password')) {
      final email = uri.queryParameters['email'];
      final token = uri.queryParameters['token'];

      if (_navigatorKey.currentState != null) {
        _navigatorKey.currentState!.push(
          MaterialPageRoute(
            builder: (context) => ResetPasswordScreen(
              initialEmail: email,
              initialToken: token,
            ),
          ),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      navigatorKey: _navigatorKey,
      title: 'Pet Center Mobile',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(
          seedColor: AppColors.primary,
          primary: AppColors.primary,
          secondary: AppColors.secondary,
        ),
        scaffoldBackgroundColor: AppColors.background,
        useMaterial3: true,
      ),
      home: const AuthWrapper(),
      routes: {
        '/login': (context) => const LoginScreen(),
        '/home': (context) => const HomeScreen(),
        '/profile': (context) => const CustomerProfileScreen(),
        '/register': (context) => const RegisterScreen(),
        '/services': (context) => const ServiceListScreen(),
        '/forgot-password': (context) => const ForgotPasswordScreen(),
        '/reset-password': (context) => const ResetPasswordScreen(),
        '/change-password': (context) => const ChangePasswordScreen(),
        '/orders': (context) => const OrderListScreen(),
        '/addresses': (context) => const AddressListScreen(),
      },
    );
  }
}

class AuthWrapper extends StatefulWidget {
  const AuthWrapper({super.key});

  @override
  State<AuthWrapper> createState() => _AuthWrapperState();
}

class _AuthWrapperState extends State<AuthWrapper> {
  bool _isLoading = true;
  bool _isLoggedIn = false;

  @override
  void initState() {
    super.initState();
    _checkAuth();
  }

  Future<void> _checkAuth() async {
    final loggedIn = await ApiService().initSession();
    if (mounted) {
      setState(() {
        _isLoggedIn = loggedIn;
        _isLoading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return const Scaffold(
        backgroundColor: AppColors.background,
        body: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(Icons.pets, size: 64, color: AppColors.primary),
              SizedBox(height: 16),
              CircularProgressIndicator(color: AppColors.primary),
            ],
          ),
        ),
      );
    }
    return _isLoggedIn ? const HomeScreen() : const LoginScreen();
  }
}
