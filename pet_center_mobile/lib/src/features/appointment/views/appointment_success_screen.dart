import 'package:flutter/material.dart';
import '../../../constants/app_colors.dart';
import '../../../widgets/custom_button.dart';
import 'appointment_detail_screen.dart';

class AppointmentSuccessScreen extends StatelessWidget {
  final String appointmentId;
  final String paymentMethod;
  final double totalAmount;

  const AppointmentSuccessScreen({
    super.key,
    required this.appointmentId,
    this.paymentMethod = 'VNPAY',
    this.totalAmount = 0.0,
  });

  @override
  Widget build(BuildContext context) {
    final displayId = appointmentId.isNotEmpty
        ? (appointmentId.length > 8 ? appointmentId.substring(0, 8).toUpperCase() : appointmentId)
        : 'N/A';

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Payment Successful'),
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
        automaticallyImplyLeading: false,
      ),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24.0),
          child: Column(
            children: [
              const SizedBox(height: 20),
              Container(
                padding: const EdgeInsets.all(20),
                decoration: const BoxDecoration(
                  color: Colors.green,
                  shape: BoxShape.circle,
                ),
                child: const Icon(
                  Icons.check,
                  size: 64,
                  color: Colors.white,
                ),
              ),
              const SizedBox(height: 24),
              const Text(
                'Appointment Deposit Paid!',
                style: TextStyle(
                  fontSize: 22,
                  fontWeight: FontWeight.bold,
                  color: AppColors.textPrimary,
                ),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 8),
              const Text(
                'Your deposit payment has been received and your appointment is now confirmed.',
                style: TextStyle(
                  fontSize: 14,
                  color: AppColors.textSecondary,
                ),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 32),

              // Appointment Details Card
              Card(
                elevation: 3,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(16),
                ),
                child: Padding(
                  padding: const EdgeInsets.all(20.0),
                  child: Column(
                    children: [
                      _buildDetailRow('Appointment ID', displayId),
                      const Divider(height: 24),
                      _buildDetailRow('Payment Method', paymentMethod),
                      const Divider(height: 24),
                      _buildDetailRow('Status', 'Confirmed', isBold: true, valueColor: Colors.green),
                      if (totalAmount > 0) ...[
                        const Divider(height: 24),
                        _buildDetailRow(
                          'Total Amount',
                          '${totalAmount.toStringAsFixed(0)}đ',
                          isBold: true,
                          valueColor: AppColors.primary,
                          fontSize: 18,
                        ),
                      ],
                    ],
                  ),
                ),
              ),
              const SizedBox(height: 40),

              // Actions
              CustomButton(
                text: 'View Appointment',
                onPressed: () {
                  Navigator.pushReplacement(
                    context,
                    MaterialPageRoute(
                      builder: (context) => AppointmentDetailScreen(
                        appointmentId: appointmentId,
                      ),
                    ),
                  );
                },
              ),
              const SizedBox(height: 12),
              OutlinedButton(
                style: OutlinedButton.styleFrom(
                  minimumSize: const Size(double.infinity, 48),
                  side: const BorderSide(color: AppColors.primary),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
                onPressed: () {
                  Navigator.pushNamedAndRemoveUntil(context, '/home', (route) => false);
                },
                child: const Text(
                  'Back to Home',
                  style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.bold,
                    color: AppColors.primary,
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildDetailRow(
    String label,
    String value, {
    bool isBold = false,
    Color? valueColor,
    double fontSize = 14,
  }) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          label,
          style: TextStyle(
            fontSize: fontSize,
            color: AppColors.textSecondary,
          ),
        ),
        const SizedBox(width: 16),
        Expanded(
          child: Text(
            value,
            textAlign: TextAlign.end,
            style: TextStyle(
              fontSize: fontSize,
              fontWeight: isBold ? FontWeight.bold : FontWeight.w600,
              color: valueColor ?? AppColors.textPrimary,
            ),
          ),
        ),
      ],
    );
  }
}
