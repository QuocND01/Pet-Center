import 'package:flutter/material.dart';
import '../constants/app_colors.dart';

class AppErrorUtils {
  /// Transforms raw technical exceptions and logs into polite, user-friendly English messages.
  static String getFriendlyMessage(dynamic error) {
    if (error == null) return 'An unexpected error occurred. Please try again later.';

    final String raw = error.toString();
    final String clean = raw.replaceAll('Exception: ', '').trim();

    // 1. Connection & Network Issues
    if (raw.contains('SocketException') ||
        raw.contains('Connection refused') ||
        raw.contains('Failed host lookup') ||
        raw.contains('Cannot connect to server') ||
        raw.contains('ClientException')) {
      return 'Unable to connect to the server. Please check your internet connection and try again.';
    }

    // 2. Timeout Issues
    if (raw.contains('TimeoutException') ||
        raw.contains('timed out') ||
        raw.contains('Connection timeout') ||
        raw.contains('Execution Timeout Expired')) {
      return 'The server connection is taking longer than expected. Please try again in a few moments.';
    }

    // 3. Server 500 & Database Failures
    if (raw.contains('SqlException') ||
        raw.contains('DbCommand') ||
        raw.contains('500') ||
        raw.contains('Internal Server Error') ||
        raw.contains('DeveloperExceptionPageMiddleware')) {
      return 'The system is currently experiencing a temporary issue. Please try again later.';
    }

    // 4. Google Sign-In Issues
    if (raw.contains('ApiException: 10') ||
        raw.contains('sign_in_failed') ||
        raw.contains('GoogleSignIn') ||
        raw.contains('redirect_uri_mismatch')) {
      return 'Google Sign-In was unsuccessful. Please try again or select another account.';
    }

    // 5. Authentication & Account Issues
    if (clean.contains('Email or password incorrect') ||
        clean.contains('InvalidCredentials')) {
      return 'Invalid email or password. Please check your credentials and try again.';
    }

    if (clean.contains('EmailNotVerified') || clean.contains('not verified')) {
      return 'Your account email is not verified. Please check your inbox for the verification link.';
    }

    if (clean.contains('AccountInactive') || clean.contains('deactivated')) {
      return 'Your account has been temporarily disabled. Please contact customer support.';
    }

    if (clean.contains('Old password incorrect') || clean.contains('wrong password')) {
      return 'The current password you entered is incorrect. Please try again.';
    }

    // 6. Return clean custom API message if it is non-technical & readable
    if (clean.isNotEmpty &&
        !clean.contains('{') &&
        !clean.contains('Stack trace') &&
        !clean.contains('http://') &&
        !clean.contains('https://') &&
        !clean.contains('System.') &&
        !clean.contains('Microsoft.') &&
        !clean.contains('at ') &&
        clean.length < 150) {
      return clean;
    }

    return 'An error occurred while processing your request. Please try again.';
  }

  /// Displays a SnackBar with a clean, user-friendly English error message
  static void showErrorSnackBar(BuildContext context, dynamic error, {Duration? duration}) {
    final message = getFriendlyMessage(error);
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(message),
        backgroundColor: AppColors.error,
        duration: duration ?? const Duration(seconds: 4),
      ),
    );
  }
}
