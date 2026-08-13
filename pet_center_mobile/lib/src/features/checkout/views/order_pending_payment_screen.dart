import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../../constants/app_colors.dart';
import '../../../services/api_service.dart';
import '../../../widgets/custom_button.dart';
import 'order_success_screen.dart';

class OrderPendingPaymentScreen extends StatefulWidget {
  final String orderId;
  final String paymentMethod;
  final String? paymentUrl;
  final double totalAmount;
  final String addressSnapshot;

  const OrderPendingPaymentScreen({
    super.key,
    required this.orderId,
    required this.paymentMethod,
    this.paymentUrl,
    required this.totalAmount,
    required this.addressSnapshot,
  });

  @override
  State<OrderPendingPaymentScreen> createState() => _OrderPendingPaymentScreenState();
}

class _OrderPendingPaymentScreenState extends State<OrderPendingPaymentScreen> {
  final ApiService _apiService = ApiService();
  bool _isChecking = false;
  bool _isCancelling = false;

  void _handleCancelOrder() async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: const Row(
          children: [
            Icon(Icons.warning_amber_rounded, color: Colors.red),
            SizedBox(width: 8),
            Text('Cancel Order', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
          ],
        ),
        content: Text('Are you sure you want to cancel order #${widget.orderId.length > 8 ? widget.orderId.substring(0, 8) : widget.orderId}?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('No, Keep Order', style: TextStyle(color: Colors.grey)),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(
              backgroundColor: Colors.red,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
            ),
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text('Yes, Cancel Order', style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );

    if (confirm == true) {
      setState(() {
        _isCancelling = true;
      });

      try {
        final success = await _apiService.cancelOrder(widget.orderId);
        if (!mounted) return;
        if (success) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('Order cancelled successfully.'),
              backgroundColor: Colors.red,
            ),
          );
          Navigator.pushNamedAndRemoveUntil(context, '/home', (route) => false);
        } else {
          _showSnackBar('Failed to cancel order. Please try again.', isError: true);
        }
      } catch (e) {
        if (mounted) {
          _showSnackBar('Failed to cancel order: $e', isError: true);
        }
      } finally {
        if (mounted) {
          setState(() {
            _isCancelling = false;
          });
        }
      }
    }
  }

  void _openPaymentGateway() async {
    if (widget.paymentUrl != null && widget.paymentUrl!.isNotEmpty) {
      final Uri uri = Uri.parse(widget.paymentUrl!);
      if (await canLaunchUrl(uri)) {
        await launchUrl(uri, mode: LaunchMode.externalApplication);
      } else {
        _showSnackBar('Cannot launch payment URL.', isError: true);
      }
    } else {
      _showSnackBar('Payment URL is unavailable for this order.', isError: true);
    }
  }

  void _checkPaymentStatus() async {
    if (_isChecking) return;

    setState(() {
      _isChecking = true;
    });

    try {
      final order = await _apiService.getOrderDetails(widget.orderId);

      if (!mounted) return;

      // paymentStatus == 2 means Paid
      if (order.paymentStatus == 2) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Payment confirmed! Order paid successfully.'),
            backgroundColor: Colors.teal,
          ),
        );

        Navigator.pushReplacement(
          context,
          MaterialPageRoute(
            builder: (context) => OrderSuccessScreen(
              orderId: widget.orderId,
              paymentMethod: widget.paymentMethod,
              totalAmount: widget.totalAmount,
              discountAmount: order.discountAmount ?? 0.0,
              finalAmount: order.totalAmount,
              addressSnapshot: widget.addressSnapshot,
            ),
          ),
        );
      } else {
        _showSnackBar(
          'Payment has not been completed yet (Status: Unpaid). Please finish payment on ${widget.paymentMethod} and try again.',
          isError: true,
        );
      }
    } catch (e) {
      if (mounted) {
        _showSnackBar('Failed to verify payment status: $e', isError: true);
      }
    } finally {
      if (mounted) {
        setState(() {
          _isChecking = false;
        });
      }
    }
  }

  void _showSnackBar(String message, {bool isError = false}) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(message),
        backgroundColor: isError ? AppColors.error : AppColors.primary,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Pending Payment'),
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
        automaticallyImplyLeading: false,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(24.0),
        child: Column(
          children: [
            const SizedBox(height: 20),
            // Header Icon
            Container(
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(
                color: Colors.orange.shade50,
                shape: BoxShape.circle,
              ),
              child: Icon(
                Icons.account_balance_wallet_outlined,
                size: 72,
                color: Colors.orange.shade800,
              ),
            ),
            const SizedBox(height: 24),
            const Text(
              'Awaiting Online Payment',
              style: TextStyle(
                fontSize: 22,
                fontWeight: FontWeight.bold,
                color: AppColors.textPrimary,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              'Your order has been created. Please complete payment via ${widget.paymentMethod}.',
              textAlign: TextAlign.center,
              style: const TextStyle(color: AppColors.textSecondary, fontSize: 14),
            ),
            const SizedBox(height: 28),

            // Order Summary Card
            Card(
              elevation: 2,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
              child: Padding(
                padding: const EdgeInsets.all(20.0),
                child: Column(
                  children: [
                    _buildRow('Order ID:', '#${widget.orderId.length > 8 ? widget.orderId.substring(0, 8) : widget.orderId}'),
                    const Divider(height: 24),
                    _buildRow('Payment Method:', widget.paymentMethod),
                    const Divider(height: 24),
                    _buildRow(
                      'Payment Status:',
                      'Unpaid (Chưa thanh toán)',
                      valueColor: Colors.orange.shade800,
                      isBold: true,
                    ),
                    const Divider(height: 24),
                    _buildRow(
                      'Total Amount:',
                      '${widget.totalAmount.toStringAsFixed(0)}đ',
                      valueColor: AppColors.primary,
                      isBold: true,
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 32),

            // Actions
            if (widget.paymentUrl != null && widget.paymentUrl!.isNotEmpty)
              ElevatedButton.icon(
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.primary,
                  foregroundColor: Colors.white,
                  minimumSize: const Size(double.infinity, 50),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                ),
                onPressed: _openPaymentGateway,
                icon: const Icon(Icons.payment),
                label: Text('Pay Now via ${widget.paymentMethod}', style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
              ),
            const SizedBox(height: 12),

            CustomButton(
              text: 'I Have Paid (Check Payment Status)',
              isLoading: _isChecking,
              onPressed: _checkPaymentStatus,
            ),
            const SizedBox(height: 12),

            OutlinedButton(
              style: OutlinedButton.styleFrom(
                minimumSize: const Size(double.infinity, 50),
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
              ),
              onPressed: () {
                Navigator.pushNamedAndRemoveUntil(context, '/home', (route) => false);
              },
              child: const Text('View Order History', style: TextStyle(fontWeight: FontWeight.bold, color: AppColors.textPrimary)),
            ),
            const SizedBox(height: 12),

            OutlinedButton.icon(
              style: OutlinedButton.styleFrom(
                foregroundColor: Colors.red,
                side: const BorderSide(color: Colors.red),
                minimumSize: const Size(double.infinity, 50),
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
              ),
              onPressed: _isCancelling ? null : _handleCancelOrder,
              icon: const Icon(Icons.cancel_outlined),
              label: Text(
                _isCancelling ? 'Cancelling Order...' : 'Cancel Order (Hủy đơn hàng này)',
                style: const TextStyle(fontWeight: FontWeight.bold),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildRow(String label, String value, {Color? valueColor, bool isBold = false}) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(label, style: const TextStyle(color: AppColors.textSecondary, fontSize: 14)),
        Flexible(
          child: Text(
            value,
            textAlign: TextAlign.end,
            style: TextStyle(
              fontSize: 14,
              fontWeight: isBold ? FontWeight.bold : FontWeight.normal,
              color: valueColor ?? AppColors.textPrimary,
            ),
          ),
        ),
      ],
    );
  }
}
