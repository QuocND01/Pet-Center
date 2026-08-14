import 'package:flutter/material.dart';
import 'package:webview_flutter/webview_flutter.dart';
import '../../../constants/app_colors.dart';

class PaymentResult {
  final bool isSuccess;
  final String? orderId;
  final String? appointmentId;
  final String? message;
  final bool userCancelled;

  PaymentResult({
    required this.isSuccess,
    this.orderId,
    this.appointmentId,
    this.message,
    this.userCancelled = false,
  });
}

class PaymentWebViewScreen extends StatefulWidget {
  final String paymentUrl;
  final String title;

  const PaymentWebViewScreen({
    super.key,
    required this.paymentUrl,
    this.title = 'Online Payment',
  });

  @override
  State<PaymentWebViewScreen> createState() => _PaymentWebViewScreenState();
}

class _PaymentWebViewScreenState extends State<PaymentWebViewScreen> {
  late final WebViewController _controller;
  bool _isLoading = true;
  bool _isHandled = false;

  @override
  void initState() {
    super.initState();
    _controller = WebViewController()
      ..setJavaScriptMode(JavaScriptMode.unrestricted)
      ..setBackgroundColor(Colors.white)
      ..setNavigationDelegate(
        NavigationDelegate(
          onPageStarted: (String url) {
            if (mounted) {
              setState(() {
                _isLoading = true;
              });
            }
            _checkUrlForPaymentResult(url);
          },
          onPageFinished: (String url) {
            if (mounted) {
              setState(() {
                _isLoading = false;
              });
            }
            _checkUrlForPaymentResult(url);
          },
          onNavigationRequest: (NavigationRequest request) {
            if (_checkUrlForPaymentResult(request.url)) {
              return NavigationDecision.prevent;
            }
            return NavigationDecision.navigate;
          },
          onWebResourceError: (WebResourceError error) {
            debugPrint('WebView Resource Error: ${error.description}');
          },
        ),
      )
      ..loadRequest(Uri.parse(widget.paymentUrl));
  }

  bool _checkUrlForPaymentResult(String url) {
    if (_isHandled) return true;

    final Uri uri = Uri.parse(url);
    final String path = uri.path;

    // Check if redirect URL matches payment return paths
    if (path.contains('/PaymentReturn') || 
        path.contains('/vnpay/return') || 
        path.contains('/momo/return') ||
        uri.queryParameters.containsKey('success')) {
      _isHandled = true;

      final bool isSuccess = uri.queryParameters['success']?.toLowerCase() == 'true' ||
          uri.queryParameters['vnp_ResponseCode'] == '00' ||
          uri.queryParameters['resultCode'] == '0';

      final String? orderId = uri.queryParameters['orderId'] ?? uri.queryParameters['vnp_TxnRef'];
      final String? appointmentId = uri.queryParameters['appointmentId'];
      final String? message = uri.queryParameters['message'];

      if (mounted) {
        Navigator.pop(
          context,
          PaymentResult(
            isSuccess: isSuccess,
            orderId: orderId,
            appointmentId: appointmentId,
            message: message,
          ),
        );
      }
      return true;
    }
    return false;
  }

  Future<bool> _confirmExit() async {
    if (_isHandled) return true;

    final bool? confirmExit = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: const Row(
          children: [
            Icon(Icons.warning_amber_rounded, color: Colors.orange),
            SizedBox(width: 8),
            Text('Cancel Payment', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
          ],
        ),
        content: const Text('Are you sure you want to cancel payment? Your order will remain pending until paid.'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('Continue Payment', style: TextStyle(color: Colors.grey)),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(
              backgroundColor: AppColors.primary,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
            ),
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text('Exit', style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );

    if (confirmExit == true && mounted) {
      Navigator.pop(
        context,
        PaymentResult(
          isSuccess: false,
          userCancelled: true,
        ),
      );
      return true;
    }
    return false;
  }

  @override
  Widget build(BuildContext context) {
    return PopScope(
      canPop: false,
      onPopInvokedWithResult: (bool didPop, dynamic result) async {
        if (didPop) return;
        await _confirmExit();
      },
      child: Scaffold(
        appBar: AppBar(
          title: Text(widget.title),
          backgroundColor: AppColors.primary,
          foregroundColor: Colors.white,
          leading: IconButton(
            icon: const Icon(Icons.close),
            onPressed: () async {
              await _confirmExit();
            },
          ),
          actions: [
            IconButton(
              icon: const Icon(Icons.refresh),
              onPressed: () {
                _controller.reload();
              },
            ),
          ],
        ),
        body: Stack(
          children: [
            WebViewWidget(controller: _controller),
            if (_isLoading)
              const Positioned(
                top: 0,
                left: 0,
                right: 0,
                child: LinearProgressIndicator(
                  color: AppColors.primary,
                  backgroundColor: Colors.white,
                ),
              ),
          ],
        ),
      ),
    );
  }
}
