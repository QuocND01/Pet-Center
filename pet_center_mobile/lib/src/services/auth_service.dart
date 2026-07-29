import 'package:shared_preferences/shared_preferences.dart';

class AuthService {
  static final AuthService _instance = AuthService._internal();
  factory AuthService() => _instance;
  AuthService._internal();

  static const String _keyToken = 'auth_token';
  static const String _keyCustomerId = 'customer_id';
  static const String _keyEmail = 'customer_email';
  static const String _keyRememberMe = 'remember_me';
  static const String _keySavedEmail = 'saved_email';
  static const String _keySavedPassword = 'saved_password';

  // Save session details
  Future<void> saveSession({
    required String token,
    required String customerId,
    required String email,
  }) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_keyToken, token);
    await prefs.setString(_keyCustomerId, customerId);
    await prefs.setString(_keyEmail, email);
  }

  // Load session token and user info
  Future<Map<String, String?>> loadSession() async {
    final prefs = await SharedPreferences.getInstance();
    return {
      'token': prefs.getString(_keyToken),
      'customerId': prefs.getString(_keyCustomerId),
      'email': prefs.getString(_keyEmail),
    };
  }

  // Clear session on logout
  Future<void> clearSession() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_keyToken);
    await prefs.remove(_keyCustomerId);
    await prefs.remove(_keyEmail);
  }

  // Save Remember Me credentials
  Future<void> saveRememberMeCredentials(String email, String password, bool remember) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool(_keyRememberMe, remember);
    if (remember) {
      await prefs.setString(_keySavedEmail, email);
      await prefs.setString(_keySavedPassword, password);
    } else {
      await prefs.remove(_keySavedEmail);
      await prefs.remove(_keySavedPassword);
    }
  }

  // Get Remember Me credentials
  Future<Map<String, dynamic>> getRememberMeCredentials() async {
    final prefs = await SharedPreferences.getInstance();
    final isRemembered = prefs.getBool(_keyRememberMe) ?? false;
    return {
      'remember': isRemembered,
      'email': isRemembered ? (prefs.getString(_keySavedEmail) ?? '') : '',
      'password': isRemembered ? (prefs.getString(_keySavedPassword) ?? '') : '',
    };
  }
}
