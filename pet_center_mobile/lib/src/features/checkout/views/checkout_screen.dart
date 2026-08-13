import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../../constants/app_colors.dart';
import '../../../models/cart_model.dart';
import '../../../models/address_model.dart';
import '../../../models/voucher_model.dart';
import '../../../services/api_service.dart';
import '../../../widgets/custom_button.dart';
import 'order_success_screen.dart';
import 'order_pending_payment_screen.dart';

class CheckoutScreen extends StatefulWidget {
  final List<CartDetailModel> selectedItems;

  const CheckoutScreen({
    super.key,
    required this.selectedItems,
  });

  @override
  State<CheckoutScreen> createState() => _CheckoutScreenState();
}

class _CheckoutScreenState extends State<CheckoutScreen> {
  final ApiService _apiService = ApiService();

  List<AddressModel> _addresses = [];
  AddressModel? _selectedAddress;

  List<VoucherModel> _vouchers = [];
  VoucherModel? _selectedVoucher;

  String _selectedPaymentMethod = 'COD'; // 'COD', 'VNPAY', 'MOMO'
  bool _isLoading = true;
  bool _isSubmitting = false;
  String? _userPhone;

  @override
  void initState() {
    super.initState();
    _loadCheckoutData();
  }

  double get _subtotal {
    double total = 0.0;
    for (var item in widget.selectedItems) {
      if (item.product != null) {
        total += item.product!.productPrice * item.quantity;
      }
    }
    return total;
  }

  double get _discountAmount {
    if (_selectedVoucher == null) return 0.0;
    return _selectedVoucher!.calculateDiscount(_subtotal);
  }

  double get _finalAmount {
    final result = _subtotal - _discountAmount;
    return result < 0 ? 0.0 : result;
  }

  Future<void> _loadCheckoutData() async {
    setState(() {
      _isLoading = true;
    });

    try {
      // 1. Check profile phone number
      final profile = await _apiService.getCustomerProfile();
      _userPhone = profile.phoneNumber;

      // 2. Load Addresses
      final addresses = await _apiService.getMyAddresses();
      _addresses = addresses;
      if (addresses.isNotEmpty) {
        _selectedAddress = addresses.firstWhere(
          (a) => a.isDefault,
          orElse: () => addresses.first,
        );
      }

      // 3. Load Available Vouchers
      final vouchers = await _apiService.getAvailableVouchers(_subtotal);
      _vouchers = vouchers;
    } catch (e) {
      debugPrint('Error loading checkout data: $e');
    } finally {
      if (mounted) {
        setState(() {
          _isLoading = false;
        });
      }
    }
  }

  void _showAddAddressDialog() {
    final formKey = GlobalKey<FormState>();
    final provinceCtrl = TextEditingController();
    final districtCtrl = TextEditingController();
    final wardCtrl = TextEditingController();
    final detailsCtrl = TextEditingController();
    bool isDefault = true;

    showDialog(
      context: context,
      builder: (ctx) {
        return AlertDialog(
          title: const Text('Add Shipping Address'),
          content: SingleChildScrollView(
            child: Form(
              key: formKey,
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  TextFormField(
                    controller: provinceCtrl,
                    decoration: const InputDecoration(labelText: 'Province / City'),
                    validator: (v) => v == null || v.isEmpty ? 'Cannot be empty' : null,
                  ),
                  TextFormField(
                    controller: districtCtrl,
                    decoration: const InputDecoration(labelText: 'District'),
                    validator: (v) => v == null || v.isEmpty ? 'Cannot be empty' : null,
                  ),
                  TextFormField(
                    controller: wardCtrl,
                    decoration: const InputDecoration(labelText: 'Ward'),
                    validator: (v) => v == null || v.isEmpty ? 'Cannot be empty' : null,
                  ),
                  TextFormField(
                    controller: detailsCtrl,
                    decoration: const InputDecoration(labelText: 'Street Name, House No.'),
                    validator: (v) => v == null || v.isEmpty ? 'Cannot be empty' : null,
                  ),
                ],
              ),
            ),
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(ctx),
              child: const Text('Cancel'),
            ),
            ElevatedButton(
              onPressed: () async {
                if (formKey.currentState!.validate()) {
                  Navigator.pop(ctx);
                  try {
                    final ok = await _apiService.addAddress(
                      province: provinceCtrl.text.trim(),
                      district: districtCtrl.text.trim(),
                      ward: wardCtrl.text.trim(),
                      addressDetails: detailsCtrl.text.trim(),
                      isDefault: isDefault,
                    );
                    if (ok) {
                      _loadCheckoutData();
                    }
                  } catch (e) {
                    _showError('Failed to add address: $e');
                  }
                }
              },
              child: const Text('Save & Select'),
            ),
          ],
        );
      },
    );
  }

  void _showSelectVoucherModal() {
    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (ctx) {
        return StatefulBuilder(
          builder: (context, setModalState) {
            return Container(
              padding: const EdgeInsets.all(20),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      const Text(
                        'Select Voucher',
                        style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                      ),
                      IconButton(
                        icon: const Icon(Icons.close),
                        onPressed: () => Navigator.pop(ctx),
                      ),
                    ],
                  ),
                  const Divider(),
                  if (_vouchers.isEmpty)
                    const Padding(
                      padding: EdgeInsets.symmetric(vertical: 24),
                      child: Center(
                        child: Text(
                          'No vouchers available for this order total.',
                          style: TextStyle(color: AppColors.textSecondary),
                        ),
                      ),
                    )
                  else
                    Expanded(
                      child: ListView.builder(
                        itemCount: _vouchers.length,
                        itemBuilder: (context, index) {
                          final v = _vouchers[index];
                          final discount = v.calculateDiscount(_subtotal);
                          final isSelected = _selectedVoucher?.voucherId == v.voucherId;

                          return Card(
                            margin: const EdgeInsets.only(bottom: 12),
                            elevation: isSelected ? 3 : 1,
                            shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(12),
                              side: BorderSide(
                                color: isSelected ? AppColors.primary : Colors.grey.shade300,
                                width: isSelected ? 2 : 1,
                              ),
                            ),
                            child: ListTile(
                              leading: const Icon(Icons.card_giftcard, color: AppColors.primary, size: 32),
                              title: Text(
                                v.code,
                                style: const TextStyle(fontWeight: FontWeight.bold),
                              ),
                              subtitle: Text(
                                '${v.description ?? "Discount ${v.discountPercent}%"}\n'
                                'Save: ${discount.toStringAsFixed(0)}đ',
                                style: const TextStyle(fontSize: 12),
                              ),
                              trailing: Radio<VoucherModel?>(
                                value: v,
                                groupValue: _selectedVoucher,
                                activeColor: AppColors.primary,
                                onChanged: (val) {
                                  setState(() {
                                    _selectedVoucher = val;
                                  });
                                  Navigator.pop(ctx);
                                },
                              ),
                            ),
                          );
                        },
                      ),
                    ),
                  if (_selectedVoucher != null)
                    SizedBox(
                      width: double.infinity,
                      child: TextButton(
                        onPressed: () {
                          setState(() {
                            _selectedVoucher = null;
                          });
                          Navigator.pop(ctx);
                        },
                        child: const Text('Remove Selected Voucher', style: TextStyle(color: Colors.red)),
                      ),
                    ),
                ],
              ),
            );
          },
        );
      },
    );
  }

  void _showMissingPhoneDialog() {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Phone Number Required'),
        content: const Text('Please update your phone number in your profile before placing an order.'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('Cancel'),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: AppColors.primary),
            onPressed: () {
              Navigator.pop(ctx);
              Navigator.pushNamed(context, '/profile');
            },
            child: const Text('Update Profile', style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );
  }

  void _handlePlaceOrder() async {
    if (_userPhone == null || _userPhone!.trim().isEmpty) {
      _showMissingPhoneDialog();
      return;
    }

    if (_selectedAddress == null) {
      _showError('Please select or add a shipping address.');
      return;
    }

    setState(() {
      _isSubmitting = true;
    });

    try {
      final result = await _apiService.placeCheckoutOrder(
        addressId: _selectedAddress!.addressId,
        items: widget.selectedItems,
        voucherId: _selectedVoucher?.voucherId,
        paymentMethod: _selectedPaymentMethod,
      );

      final isSuccess = result['success'] == true || result['Success'] == true || result['orderId'] != null || result['OrderId'] != null;
      final String? paymentUrl = result['paymentUrl'] ?? result['PaymentUrl'];
      final String orderId = (result['orderId'] ?? result['OrderId'] ?? '').toString();

      if (isSuccess || (paymentUrl != null && paymentUrl.isNotEmpty)) {
        if (_selectedPaymentMethod == 'COD') {
          if (!mounted) return;
          Navigator.pushReplacement(
            context,
            MaterialPageRoute(
              builder: (context) => OrderSuccessScreen(
                orderId: orderId,
                paymentMethod: _selectedPaymentMethod,
                totalAmount: _subtotal,
                discountAmount: _discountAmount,
                finalAmount: _finalAmount,
                addressSnapshot: _selectedAddress!.fullAddress,
              ),
            ),
          );
        } else {
          // Online Payment (VNPAY / MOMO)
          if (paymentUrl != null && paymentUrl.isNotEmpty) {
            final Uri uri = Uri.parse(paymentUrl);
            if (await canLaunchUrl(uri)) {
              await launchUrl(uri, mode: LaunchMode.externalApplication);
            }
          }

          if (!mounted) return;
          Navigator.pushReplacement(
            context,
            MaterialPageRoute(
              builder: (context) => OrderPendingPaymentScreen(
                orderId: orderId,
                paymentMethod: _selectedPaymentMethod,
                paymentUrl: paymentUrl,
                totalAmount: _finalAmount,
                addressSnapshot: _selectedAddress!.fullAddress,
              ),
            ),
          );
        }
      } else {
        _showError(result['message'] ?? result['Message'] ?? 'Order placement failed.');
      }
    } catch (e) {
      _showError('Order placement failed: $e');
    } finally {
      if (mounted) {
        setState(() {
          _isSubmitting = false;
        });
      }
    }
  }

  void _showError(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message), backgroundColor: AppColors.error),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Checkout'),
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : Column(
              children: [
                Expanded(
                  child: SingleChildScrollView(
                    padding: const EdgeInsets.all(16.0),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        // 1. Shipping Address Section
                        _buildSectionHeader(Icons.location_on_outlined, 'Shipping Address'),
                        Card(
                          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                          child: Padding(
                            padding: const EdgeInsets.all(16.0),
                            child: _selectedAddress == null
                                ? Row(
                                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                    children: [
                                      const Text('No shipping address selected'),
                                      ElevatedButton(
                                        onPressed: _showAddAddressDialog,
                                        child: const Text('Add New'),
                                      ),
                                    ],
                                  )
                                : Column(
                                    crossAxisAlignment: CrossAxisAlignment.start,
                                    children: [
                                      Row(
                                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                        children: [
                                          Text(
                                            _selectedAddress!.fullAddress,
                                            style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                                          ),
                                          TextButton(
                                            onPressed: () {
                                              _showSelectAddressModal();
                                            },
                                            child: const Text('Change'),
                                          ),
                                        ],
                                      ),
                                    ],
                                  ),
                          ),
                        ),
                        const SizedBox(height: 16),

                        // 2. Order Items Summary Section
                        _buildSectionHeader(Icons.shopping_bag_outlined, 'Order Items (${widget.selectedItems.length})'),
                        Card(
                          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                          child: ListView.separated(
                            shrinkWrap: true,
                            physics: const NeverScrollableScrollPhysics(),
                            itemCount: widget.selectedItems.length,
                            separatorBuilder: (_, __) => const Divider(height: 1),
                            itemBuilder: (context, index) {
                              final item = widget.selectedItems[index];
                              final product = item.product;
                              return ListTile(
                                leading: Container(
                                  width: 48,
                                  height: 48,
                                  decoration: BoxDecoration(
                                    borderRadius: BorderRadius.circular(8),
                                    color: Colors.grey.shade100,
                                  ),
                                  child: product != null && product.images.isNotEmpty
                                      ? Image.network(product.images.first, fit: BoxFit.cover)
                                      : const Icon(Icons.pets, color: Colors.grey),
                                ),
                                title: Text(
                                  product?.productName ?? 'Product',
                                  style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 14),
                                  maxLines: 1,
                                  overflow: TextOverflow.ellipsis,
                                ),
                                subtitle: Text('Qty: ${item.quantity} x ${product?.productPrice.toStringAsFixed(0)}đ'),
                                trailing: Text(
                                  '${((product?.productPrice ?? 0.0) * item.quantity).toStringAsFixed(0)}đ',
                                  style: const TextStyle(fontWeight: FontWeight.bold, color: AppColors.primary),
                                ),
                              );
                            },
                          ),
                        ),
                        const SizedBox(height: 16),

                        // 3. Voucher Section
                        _buildSectionHeader(Icons.card_giftcard_outlined, 'Voucher Discount'),
                        Card(
                          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                          child: ListTile(
                            leading: const Icon(Icons.confirmation_number_outlined, color: AppColors.primary),
                            title: Text(
                              _selectedVoucher != null ? 'Code: ${_selectedVoucher!.code}' : 'Apply Voucher',
                              style: const TextStyle(fontWeight: FontWeight.bold),
                            ),
                            subtitle: _selectedVoucher != null
                                ? Text('Discount: -${_discountAmount.toStringAsFixed(0)}đ', style: const TextStyle(color: Colors.green))
                                : const Text('Select an available voucher'),
                            trailing: const Icon(Icons.chevron_right),
                            onTap: _showSelectVoucherModal,
                          ),
                        ),
                        const SizedBox(height: 16),

                        // 4. Payment Method Section
                        _buildSectionHeader(Icons.payment_outlined, 'Payment Method'),
                        Card(
                          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                          child: Column(
                            children: [
                              RadioListTile<String>(
                                title: const Text('Cash on Delivery (COD)'),
                                subtitle: const Text('Pay when you receive your order'),
                                secondary: const Icon(Icons.money, color: Colors.green),
                                value: 'COD',
                                groupValue: _selectedPaymentMethod,
                                activeColor: AppColors.primary,
                                onChanged: (val) {
                                  setState(() {
                                    _selectedPaymentMethod = val!;
                                  });
                                },
                              ),
                              const Divider(height: 1),
                              RadioListTile<String>(
                                title: const Text('VNPAY Gateway'),
                                subtitle: const Text('Pay securely via VNPAY QR / Banking'),
                                secondary: const Icon(Icons.account_balance_wallet, color: Colors.blue),
                                value: 'VNPAY',
                                groupValue: _selectedPaymentMethod,
                                activeColor: AppColors.primary,
                                onChanged: (val) {
                                  setState(() {
                                    _selectedPaymentMethod = val!;
                                  });
                                },
                              ),
                              const Divider(height: 1),
                              RadioListTile<String>(
                                title: const Text('MoMo E-Wallet'),
                                subtitle: const Text('Pay with MoMo Wallet'),
                                secondary: const Icon(Icons.qr_code, color: Colors.pink),
                                value: 'MOMO',
                                groupValue: _selectedPaymentMethod,
                                activeColor: AppColors.primary,
                                onChanged: (val) {
                                  setState(() {
                                    _selectedPaymentMethod = val!;
                                  });
                                },
                              ),
                            ],
                          ),
                        ),
                        const SizedBox(height: 16),

                        // 5. Payment Breakdown Summary Card
                        Card(
                          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                          child: Padding(
                            padding: const EdgeInsets.all(16.0),
                            child: Column(
                              children: [
                                _buildSummaryRow('Subtotal', '${_subtotal.toStringAsFixed(0)}đ'),
                                const SizedBox(height: 8),
                                _buildSummaryRow('Voucher Discount', '-${_discountAmount.toStringAsFixed(0)}đ', textColor: Colors.green),
                                const Divider(height: 24),
                                _buildSummaryRow(
                                  'Total Payment',
                                  '${_finalAmount.toStringAsFixed(0)}đ',
                                  isBold: true,
                                  textColor: AppColors.primary,
                                  fontSize: 18,
                                ),
                              ],
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),

                // Bottom Place Order Bar
                Card(
                  margin: EdgeInsets.zero,
                  elevation: 8,
                  shape: const RoundedRectangleBorder(
                    borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
                  ),
                  child: Padding(
                    padding: const EdgeInsets.all(20.0),
                    child: Row(
                      children: [
                        Expanded(
                          child: Column(
                            mainAxisSize: MainAxisSize.min,
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              const Text('Total Payment', style: TextStyle(fontSize: 12, color: AppColors.textSecondary)),
                              Text(
                                '${_finalAmount.toStringAsFixed(0)}đ',
                                style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: AppColors.primary),
                              ),
                            ],
                          ),
                        ),
                        Expanded(
                          child: CustomButton(
                            text: 'Place Order',
                            isLoading: _isSubmitting,
                            onPressed: _handlePlaceOrder,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
    );
  }

  void _showSelectAddressModal() {
    showModalBottomSheet(
      context: context,
      builder: (ctx) {
        return Container(
          padding: const EdgeInsets.all(20),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  const Text('Select Address', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                  TextButton(
                    onPressed: () {
                      Navigator.pop(ctx);
                      _showAddAddressDialog();
                    },
                    child: const Text('Add New'),
                  ),
                ],
              ),
              const Divider(),
              Expanded(
                child: ListView.builder(
                  itemCount: _addresses.length,
                  itemBuilder: (context, index) {
                    final addr = _addresses[index];
                    return RadioListTile<AddressModel>(
                      title: Text(addr.fullAddress),
                      value: addr,
                      groupValue: _selectedAddress,
                      onChanged: (val) {
                        setState(() {
                          _selectedAddress = val;
                        });
                        Navigator.pop(ctx);
                      },
                    );
                  },
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _buildSectionHeader(IconData icon, String title) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 8.0, top: 4.0),
      child: Row(
        children: [
          Icon(icon, size: 20, color: AppColors.primary),
          const SizedBox(width: 8),
          Text(
            title,
            style: const TextStyle(fontSize: 15, fontWeight: FontWeight.bold, color: AppColors.textPrimary),
          ),
        ],
      ),
    );
  }

  Widget _buildSummaryRow(String label, String value, {bool isBold = false, Color? textColor, double fontSize = 14}) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(label, style: TextStyle(fontSize: fontSize, color: AppColors.textSecondary)),
        Text(
          value,
          style: TextStyle(
            fontSize: fontSize,
            fontWeight: isBold ? FontWeight.bold : FontWeight.w600,
            color: textColor ?? AppColors.textPrimary,
          ),
        ),
      ],
    );
  }
}
