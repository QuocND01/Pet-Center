import 'package:flutter/material.dart';

class AppColors {
  static const Color primary = Color(0xFF00796B); // Deep Teal
  static const Color primaryDark = Color(0xFF004D40);
  static const Color primaryLight = Color(0xFF4DB6AC);
  static const Color secondary = Color(0xFF009688);
  static const Color accent = Color(0xFFFFB74D); // Soft Warm Orange accent for pets
  static const Color background = Color(0xFFF7F9FC);
  static const Color cardBg = Colors.white;
  static const Color textPrimary = Color(0xFF1E293B);
  static const Color textSecondary = Color(0xFF64748B);
  static const Color inputBorder = Color(0xFFE2E8F0);
  static const Color error = Color(0xFFEF4444);
  static const Color success = Color(0xFF10B981);

  static const LinearGradient primaryGradient = LinearGradient(
    colors: [Color(0xFF00796B), Color(0xFF004D40)],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );
}

