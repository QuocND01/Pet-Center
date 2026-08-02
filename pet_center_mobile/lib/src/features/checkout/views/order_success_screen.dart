import 'package:flutter/material.dart';
import '../../../constants/app_colors.dart';
import '../../../widgets/custom_button.dart';

class OrderSuccessScreen extends StatelessWidget {
  final String orderId;
  final String paymentMethod;
  final double totalAmount;
  final double discountAmount;
  final double finalAmount;
  final String addressSnapshot;

  const OrderSuccessScreen({
    super.key,
    required this.orderId,
    this.paymentMethod = 'COD',
    required this.totalAmount,
    this.discountAmount = 0.0,
    required this.finalAmount,
    this.addressSnapshot = '',
  });

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Order Placed Successfully'),
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
                'Thank You for Your Order!',
                style: TextStyle(
                  fontSize: 22,
                  fontWeight: FontWeight.bold,
                  color: AppColors.textPrimary,
                ),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 8),
              const Text(
                'Your order has been received and is being processed.',
                style: TextStyle(
                  fontSize: 14,
                  color: AppColors.textSecondary,
                ),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 32),

              // Order Details Card
              Card(
                elevation: 3,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(16),
                ),
                child: Padding(
                  padding: const EdgeInsets.all(20.0),
                  child: Column(
                    children: [
                      _buildDetailRow('Order ID', orderId.isNotEmpty ? orderId.substring(0, 8).toUpperCase() : 'N/A'),
                      const Divider(height: 24),
                      _buildDetailRow('Payment Method', paymentMethod),
                      if (addressSnapshot.isNotEmpty) ...[
                        const Divider(height: 24),
                        _buildDetailRow('Shipping Address', addressSnapshot),
                      ],
                      const Divider(height: 24),
                      _buildDetailRow('Subtotal', '${totalAmount.toStringAsFixed(0)}đ'),
                      if (discountAmount > 0) ...[
                        const SizedBox(height: 8),
                        _buildDetailRow('Voucher Discount', '-${discountAmount.toStringAsFixed(0)}đ', valueColor: Colors.green),
                      ],
                      const Divider(height: 24),
                      _buildDetailRow(
                        'Total Paid',
                        '${finalAmount.toStringAsFixed(0)}đ',
                        isBold: true,
                        valueColor: AppColors.primary,
                        fontSize: 18,
                      ),
                    ],
                  ),
                ),
              ),
              const SizedBox(height: 40),

              // Actions
              CustomButton(
                text: 'View My Orders',
                onPressed: () {
                  Navigator.pushNamedAndRemoveUntil(context, '/orders', (route) => false);
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
