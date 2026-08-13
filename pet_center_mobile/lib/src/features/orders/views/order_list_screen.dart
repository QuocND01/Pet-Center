import 'package:flutter/material.dart';
import '../../../constants/app_colors.dart';
import '../../../models/order_model.dart';
import '../../../services/api_service.dart';
import '../widgets/view_order_feedback_sheet.dart';
import '../widgets/write_order_feedback_sheet.dart';

class OrderListScreen extends StatefulWidget {
  const OrderListScreen({super.key});

  @override
  State<OrderListScreen> createState() => _OrderListScreenState();
}

class _OrderListScreenState extends State<OrderListScreen> {
  final ApiService _apiService = ApiService();
  late Future<List<OrderModel>> _ordersFuture;
  List<OrderModel> _allOrders = [];
  int _selectedStatusFilter =
      -1; // -1 = All, 0 = Cancelled, 1 = Pending, 2 = Processing, 3 = Shipping, 4 = Completed

  @override
  void initState() {
    super.initState();
    _loadOrders();
  }

  void _loadOrders() {
    setState(() {
      _ordersFuture = _apiService.getMyOrders().then((list) {
        if (mounted) {
          setState(() {
            _allOrders = list;
          });
        }
        return list;
      });
    });
  }

  List<OrderModel> get _filteredOrders {
    if (_selectedStatusFilter == -1) return _allOrders;
    return _allOrders.where((o) => o.status == _selectedStatusFilter).toList();
  }

  String _formatDate(DateTime dt) {
    final d = dt.day.toString().padLeft(2, '0');
    final m = dt.month.toString().padLeft(2, '0');
    final y = dt.year;
    final h = dt.hour.toString().padLeft(2, '0');
    final min = dt.minute.toString().padLeft(2, '0');
    return '$d/$m/$y $h:$min';
  }

  String _getPaymentStatusText(int paymentStatus) {
    switch (paymentStatus) {
      case 1:
        return 'Unpaid';
      case 2:
        return 'Paid';
      case 0:
        return 'Pending';
      case 3:
        return 'Refunded';
      default:
        return 'Unknown';
    }
  }

  String _formatCurrency(double amount) {
    return '${amount.toStringAsFixed(0)}đ';
  }

  double _getTotalSpend(List<OrderModel> orders) {
    return orders.fold<double>(0, (sum, order) => sum + order.totalAmount);
  }

  String _getShortAddress(String address) {
    if (address.trim().isEmpty) return 'No address info';
    final normalized = address.trim().replaceAll(RegExp(r'\s+'), ' ');
    if (normalized.length <= 70) return normalized;
    return '${normalized.substring(0, 67)}...';
  }

  Color _getStatusColor(int status) {
    switch (status) {
      case 0:
        return Colors.red.shade700; // Cancelled
      case 1:
        return Colors.orange.shade800; // Pending
      case 2:
        return Colors.blue.shade700; // Processing
      case 3:
        return AppColors.primary; // Delivering
      case 4:
        return Colors.green.shade800; // Completed
      default:
        return Colors.grey.shade700;
    }
  }

  Color _getStatusBgColor(int status) {
    switch (status) {
      case 0:
        return Colors.red.shade50;
      case 1:
        return Colors.orange.shade50;
      case 2:
        return Colors.blue.shade50;
      case 3:
        return AppColors.primary.withValues(alpha: 0.12);
      case 4:
        return Colors.green.shade50;
      default:
        return Colors.grey.shade100;
    }
  }

  void _handleCancelOrder(OrderModel order) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: const Row(
          children: [
            Icon(Icons.warning_amber_rounded, color: Colors.red),
            SizedBox(width: 8),
            Text('Cancel Order',
                style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
          ],
        ),
        content: Text(
            'Are you sure you want to cancel order #${order.orderId.substring(0, 8).toUpperCase()}?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child:
                const Text('No, Keep It', style: TextStyle(color: Colors.grey)),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(
              backgroundColor: Colors.red,
              shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(8)),
            ),
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text('Yes, Cancel Order',
                style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );

    if (confirm == true) {
      try {
        final success = await _apiService.cancelOrder(order.orderId);
        if (mounted) {
          if (success) {
            ScaffoldMessenger.of(context).showSnackBar(
              const SnackBar(
                content: Text('Order cancelled successfully.'),
                backgroundColor: Colors.green,
              ),
            );
            _loadOrders();
          } else {
            ScaffoldMessenger.of(context).showSnackBar(
              const SnackBar(
                content: Text('Failed to cancel order.'),
                backgroundColor: AppColors.error,
              ),
            );
          }
        }
      } catch (e) {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text('Error: $e'),
              backgroundColor: AppColors.error,
            ),
          );
        }
      }
    }
  }

  void _showOrderDetailsModal(OrderModel orderSummary) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (ctx) => Container(
        height: MediaQuery.of(context).size.height * 0.85,
        decoration: const BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.only(
            topLeft: Radius.circular(24),
            topRight: Radius.circular(24),
          ),
        ),
        child: FutureBuilder<OrderModel>(
          future: _apiService.getOrderDetails(orderSummary.orderId),
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const Center(
                  child: CircularProgressIndicator(color: AppColors.primary));
            }

            final order = snapshot.data ?? orderSummary;

            return Column(
              children: [
                // Top handle
                const SizedBox(height: 12),
                Container(
                  width: 40,
                  height: 4,
                  decoration: BoxDecoration(
                    color: Colors.grey.shade300,
                    borderRadius: BorderRadius.circular(2),
                  ),
                ),
                const SizedBox(height: 16),

                Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 20),
                  child: Container(
                    padding: const EdgeInsets.all(14),
                    decoration: BoxDecoration(
                      gradient: const LinearGradient(
                        colors: [Color(0xFF0F766E), Color(0xFF00796B)],
                        begin: Alignment.topLeft,
                        end: Alignment.bottomRight,
                      ),
                      borderRadius: BorderRadius.circular(18),
                    ),
                    child: Row(
                      children: [
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                'Order #${order.orderId.substring(0, 8).toUpperCase()}',
                                style: const TextStyle(
                                    fontSize: 16,
                                    fontWeight: FontWeight.bold,
                                    color: Colors.white),
                              ),
                              const SizedBox(height: 4),
                              Text(
                                _formatDate(order.orderDate),
                                style: const TextStyle(
                                    fontSize: 12, color: Colors.white70),
                              ),
                            ],
                          ),
                        ),
                        Container(
                          padding: const EdgeInsets.symmetric(
                              horizontal: 10, vertical: 5),
                          decoration: BoxDecoration(
                            color: Colors.white.withValues(alpha: 0.18),
                            borderRadius: BorderRadius.circular(999),
                          ),
                          child: Text(
                            order.statusText,
                            style: const TextStyle(
                                fontSize: 11,
                                fontWeight: FontWeight.bold,
                                color: Colors.white),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 12),

                // Quick Summary Chips
                Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 20),
                  child: SingleChildScrollView(
                    scrollDirection: Axis.horizontal,
                    child: Row(
                      children: [
                        _buildModalSummaryChip(
                            Icons.credit_card_outlined, order.paymentMethod),
                        const SizedBox(width: 8),
                        _buildModalSummaryChip(
                          Icons.payments_outlined,
                          _getPaymentStatusText(order.paymentStatus),
                          badgeColor: order.paymentStatus == 1
                              ? Colors.green
                              : Colors.orange,
                        ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 16),

                // Scrollable Content
                Expanded(
                  child: SingleChildScrollView(
                    padding: const EdgeInsets.symmetric(horizontal: 20),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        // Customer & Shipping Address Info
                        Container(
                          padding: const EdgeInsets.all(14),
                          decoration: BoxDecoration(
                            color: Colors.grey.shade50,
                            borderRadius: BorderRadius.circular(12),
                            border: Border.all(color: Colors.grey.shade200),
                          ),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              const Row(
                                children: [
                                  Icon(Icons.location_on_outlined,
                                      color: AppColors.primary, size: 18),
                                  SizedBox(width: 6),
                                  Text(
                                    'Shipping Address',
                                    style: TextStyle(
                                        fontWeight: FontWeight.bold,
                                        fontSize: 14),
                                  ),
                                ],
                              ),
                              const SizedBox(height: 8),
                              Text(
                                '${order.customerName} (${order.phoneNumber.isNotEmpty ? order.phoneNumber : 'No phone'})',
                                style: const TextStyle(
                                    fontWeight: FontWeight.bold, fontSize: 13),
                              ),
                              const SizedBox(height: 4),
                              Text(
                                order.addressSnapshot.isNotEmpty
                                    ? order.addressSnapshot
                                    : 'No address info',
                                style: const TextStyle(
                                    fontSize: 12,
                                    color: AppColors.textSecondary,
                                    height: 1.4),
                              ),
                            ],
                          ),
                        ),
                        const SizedBox(height: 16),
                        Container(
                          padding: const EdgeInsets.all(12),
                          decoration: BoxDecoration(
                            color: AppColors.primary.withValues(alpha: 0.05),
                            borderRadius: BorderRadius.circular(14),
                          ),
                          child: Row(
                            children: [
                              Expanded(
                                child: _buildDetailMetric(
                                    'Items', '${order.orderItems.length}'),
                              ),
                              const SizedBox(width: 8),
                              Expanded(
                                child: _buildDetailMetric(
                                    'Payment', order.paymentMethod),
                              ),
                              const SizedBox(width: 8),
                              Expanded(
                                child: _buildDetailMetric(
                                    'Status', order.statusText),
                              ),
                            ],
                          ),
                        ),
                        const SizedBox(height: 16),

                        const Text(
                          'Order Items',
                          style: TextStyle(
                              fontSize: 15, fontWeight: FontWeight.bold),
                        ),
                        const SizedBox(height: 10),
                        if (order.orderItems.isEmpty)
                          const Padding(
                            padding: EdgeInsets.symmetric(vertical: 16),
                            child: Text('No item details available.',
                                style: TextStyle(color: Colors.grey)),
                          )
                        else
                          ListView.separated(
                            shrinkWrap: true,
                            physics: const NeverScrollableScrollPhysics(),
                            itemCount: order.orderItems.length,
                            separatorBuilder: (context, index) =>
                                const Divider(height: 16),
                            itemBuilder: (context, index) {
                              final item = order.orderItems[index];
                              return Container(
                                padding: const EdgeInsets.all(10),
                                decoration: BoxDecoration(
                                  color: Colors.white,
                                  borderRadius: BorderRadius.circular(12),
                                  border:
                                      Border.all(color: Colors.grey.shade200),
                                ),
                                child: Row(
                                  children: [
                                    ClipRRect(
                                      borderRadius: BorderRadius.circular(8),
                                      child: Container(
                                        width: 56,
                                        height: 56,
                                        color: Colors.grey.shade100,
                                        child: item.productImage != null &&
                                                item.productImage!.isNotEmpty
                                            ? Image.network(
                                                item.productImage!,
                                                fit: BoxFit.cover,
                                                errorBuilder: (context, error,
                                                        stackTrace) =>
                                                    const Icon(Icons.pets,
                                                        color: Colors.grey),
                                              )
                                            : const Icon(Icons.pets,
                                                color: Colors.grey),
                                      ),
                                    ),
                                    const SizedBox(width: 12),
                                    Expanded(
                                      child: Column(
                                        crossAxisAlignment:
                                            CrossAxisAlignment.start,
                                        children: [
                                          Text(
                                            item.productName,
                                            style: const TextStyle(
                                                fontWeight: FontWeight.bold,
                                                fontSize: 14),
                                            maxLines: 2,
                                            overflow: TextOverflow.ellipsis,
                                          ),
                                          const SizedBox(height: 4),
                                          Text(
                                            '${item.unitPrice.toStringAsFixed(0)}đ × ${item.quantity}',
                                            style: const TextStyle(
                                                fontSize: 12,
                                                color: AppColors.textSecondary),
                                          ),
                                        ],
                                      ),
                                    ),
                                    Text(
                                      '${item.subTotal.toStringAsFixed(0)}đ',
                                      style: const TextStyle(
                                          fontWeight: FontWeight.bold,
                                          fontSize: 14,
                                          color: AppColors.primary),
                                    ),
                                  ],
                                ),
                              );
                            },
                          ),
                        const SizedBox(height: 20),

                        // Payment Summary Card
                        Container(
                          padding: const EdgeInsets.all(14),
                          decoration: BoxDecoration(
                            color: Colors.grey.shade50,
                            borderRadius: BorderRadius.circular(12),
                            border: Border.all(color: Colors.grey.shade200),
                          ),
                          child: Column(
                            children: [
                              Row(
                                mainAxisAlignment:
                                    MainAxisAlignment.spaceBetween,
                                children: [
                                  const Text('Payment Method',
                                      style: TextStyle(
                                          fontSize: 13,
                                          color: AppColors.textSecondary)),
                                  Text(order.paymentMethod,
                                      style: const TextStyle(
                                          fontSize: 13,
                                          fontWeight: FontWeight.bold)),
                                ],
                              ),
                              const SizedBox(height: 8),
                              Row(
                                mainAxisAlignment:
                                    MainAxisAlignment.spaceBetween,
                                children: [
                                  const Text('Payment Status',
                                      style: TextStyle(
                                          fontSize: 13,
                                          color: AppColors.textSecondary)),
                                  Text(
                                    _getPaymentStatusText(order.paymentStatus),
                                    style: TextStyle(
                                      fontSize: 13,
                                      fontWeight: FontWeight.bold,
                                      color: order.paymentStatus == 1
                                          ? AppColors.success
                                          : AppColors.error,
                                    ),
                                  ),
                                ],
                              ),
                              const SizedBox(height: 10),
                              const Divider(height: 1),
                              const SizedBox(height: 10),
                              Row(
                                mainAxisAlignment:
                                    MainAxisAlignment.spaceBetween,
                                children: [
                                  const Text('Total Amount',
                                      style: TextStyle(
                                          fontSize: 15,
                                          fontWeight: FontWeight.bold)),
                                  Text(
                                    _formatCurrency(order.totalAmount),
                                    style: const TextStyle(
                                        fontSize: 18,
                                        fontWeight: FontWeight.bold,
                                        color: AppColors.primary),
                                  ),
                                ],
                              ),
                            ],
                          ),
                        ),
                        const SizedBox(height: 16),
                        _buildFeedbackSection(order),
                        const SizedBox(height: 24),
                      ],
                    ),
                  ),
                ),
              ],
            );
          },
        ),
      ),
    );
  }

  Widget _buildDetailMetric(String title, String value) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(title,
            style:
                const TextStyle(fontSize: 11, color: AppColors.textSecondary)),
        const SizedBox(height: 2),
        Text(
          value,
          style: const TextStyle(
              fontSize: 12,
              fontWeight: FontWeight.bold,
              color: AppColors.textPrimary),
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
        ),
      ],
    );
  }

  Widget _buildFeedbackSection(OrderModel order) {
    if (order.status != 4) return const SizedBox.shrink();

    return FutureBuilder<bool>(
      future: _apiService.checkHasFeedback(order.orderId),
      builder: (context, snapshot) {
        if (snapshot.connectionState == ConnectionState.waiting) {
          return const Center(
            child: Padding(
              padding: EdgeInsets.symmetric(vertical: 8.0),
              child: CircularProgressIndicator(
                  color: AppColors.primary, strokeWidth: 2),
            ),
          );
        }

        final hasFeedback = snapshot.data ?? false;

        return SizedBox(
          width: double.infinity,
          child: hasFeedback
              ? OutlinedButton.icon(
                  style: OutlinedButton.styleFrom(
                    foregroundColor: AppColors.primary,
                    side:
                        const BorderSide(color: AppColors.primary, width: 1.5),
                    shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12)),
                    padding: const EdgeInsets.symmetric(vertical: 12),
                  ),
                  onPressed: () {
                    Navigator.pop(context); // Close detail modal
                    showModalBottomSheet(
                      context: context,
                      isScrollControlled: true,
                      backgroundColor: Colors.transparent,
                      builder: (ctx) => ViewOrderFeedbackSheet(order: order),
                    );
                  },
                  icon: const Icon(Icons.star_outline_rounded, size: 20),
                  label: const Text('View Reviews',
                      style:
                          TextStyle(fontWeight: FontWeight.bold, fontSize: 14)),
                )
              : ElevatedButton.icon(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.amber.shade700,
                    foregroundColor: Colors.white,
                    shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12)),
                    padding: const EdgeInsets.symmetric(vertical: 12),
                    elevation: 2,
                  ),
                  onPressed: () async {
                    Navigator.pop(context); // Close detail modal
                    final refresh = await showModalBottomSheet<bool>(
                      context: context,
                      isScrollControlled: true,
                      backgroundColor: Colors.transparent,
                      builder: (ctx) => WriteOrderFeedbackSheet(order: order),
                    );
                    if (refresh == true) {
                      _loadOrders();
                    }
                  },
                  icon: const Icon(Icons.rate_review_rounded, size: 20),
                  label: const Text('Write a Review',
                      style:
                          TextStyle(fontWeight: FontWeight.bold, fontSize: 14)),
                ),
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    if (_apiService.token == null) {
      return Scaffold(
        appBar: AppBar(
            title: const Text('My Orders'),
            backgroundColor: AppColors.primary,
            foregroundColor: Colors.white),
        body: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Icon(Icons.receipt_long_outlined,
                  size: 80, color: Colors.grey),
              const SizedBox(height: 16),
              const Text('Please login to view your order history.'),
              const SizedBox(height: 16),
              ElevatedButton(
                onPressed: () => Navigator.pushNamed(context, '/login'),
                child: const Text('Login Now'),
              ),
            ],
          ),
        ),
      );
    }

    final filtered = _filteredOrders;

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('My Orders',
            style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
        automaticallyImplyLeading: false,
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
        elevation: 0,
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: _loadOrders,
          ),
        ],
      ),
      body: Column(
        children: [
          _buildSummaryHeader(filtered.length, _getTotalSpend(_allOrders)),
          Container(
            color: Colors.white,
            padding: const EdgeInsets.symmetric(vertical: 10, horizontal: 16),
            child: SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              child: Row(
                children: [
                  _buildFilterChip('All (${_allOrders.length})', -1),
                  _buildFilterChip(
                      'Pending (${_allOrders.where((o) => o.status == 1).length})',
                      1),
                  _buildFilterChip(
                      'Processing (${_allOrders.where((o) => o.status == 2).length})',
                      2),
                  _buildFilterChip(
                      'Delivering (${_allOrders.where((o) => o.status == 3).length})',
                      3),
                  _buildFilterChip(
                      'Completed (${_allOrders.where((o) => o.status == 4).length})',
                      4),
                  _buildFilterChip(
                      'Cancelled (${_allOrders.where((o) => o.status == 0).length})',
                      0),
                ],
              ),
            ),
          ),

          // Main Order Cards List
          Expanded(
            child: RefreshIndicator(
              onRefresh: () async => _loadOrders(),
              color: AppColors.primary,
              child: FutureBuilder<List<OrderModel>>(
                future: _ordersFuture,
                builder: (context, snapshot) {
                  if (snapshot.connectionState == ConnectionState.waiting) {
                    return const Center(
                        child: CircularProgressIndicator(
                            color: AppColors.primary));
                  }

                  if (snapshot.hasError) {
                    return Center(
                      child: Padding(
                        padding: const EdgeInsets.all(24.0),
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            const Icon(Icons.error_outline,
                                size: 64, color: AppColors.error),
                            const SizedBox(height: 16),
                            Text('Error: ${snapshot.error}',
                                style: const TextStyle(color: AppColors.error)),
                            const SizedBox(height: 16),
                            ElevatedButton(
                              onPressed: _loadOrders,
                              child: const Text('Try Again'),
                            ),
                          ],
                        ),
                      ),
                    );
                  }

                  if (filtered.isEmpty) {
                    return ListView(
                      children: [
                        SizedBox(
                          height: MediaQuery.of(context).size.height * 0.5,
                          child: const Center(
                            child: Column(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                Text('🐾', style: TextStyle(fontSize: 48)),
                                SizedBox(height: 12),
                                Text(
                                  "You haven't placed any orders yet.",
                                  style: TextStyle(
                                      fontSize: 16,
                                      fontWeight: FontWeight.bold,
                                      color: AppColors.textPrimary),
                                ),
                                SizedBox(height: 4),
                                Text(
                                  'Start shopping to see your orders here!',
                                  style: TextStyle(
                                      fontSize: 12,
                                      color: AppColors.textSecondary),
                                ),
                              ],
                            ),
                          ),
                        ),
                      ],
                    );
                  }

                  return ListView.builder(
                    padding: const EdgeInsets.all(16),
                    itemCount: filtered.length,
                    itemBuilder: (context, index) {
                      final order = filtered[index];
                      return _buildOrderCard(order);
                    },
                  );
                },
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildSummaryHeader(int visibleCount, double totalSpend) {
    final completed = _allOrders.where((o) => o.status == 4).length;
    final pending = _allOrders.where((o) => o.status == 1).length;

    return Container(
      margin: const EdgeInsets.fromLTRB(16, 16, 16, 8),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(20),
        gradient: const LinearGradient(
          colors: [Color(0xFF0F766E), Color(0xFF00796B)],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.08),
            blurRadius: 16,
            offset: const Offset(0, 8),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              const Icon(Icons.receipt_long, color: Colors.white, size: 20),
              const SizedBox(width: 8),
              const Expanded(
                child: Text(
                  'Order overview',
                  style: TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.bold,
                      color: Colors.white),
                ),
              ),
              Container(
                padding:
                    const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                decoration: BoxDecoration(
                  color: Colors.white.withValues(alpha: 0.16),
                  borderRadius: BorderRadius.circular(999),
                ),
                child: Text(
                  '$visibleCount shown',
                  style: const TextStyle(
                      fontSize: 11,
                      color: Colors.white,
                      fontWeight: FontWeight.bold),
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Row(
            children: [
              Expanded(
                child: _buildSummaryMetric(
                    'Total spend', _formatCurrency(totalSpend)),
              ),
              const SizedBox(width: 10),
              Expanded(
                child: _buildSummaryMetric('Completed', '$completed'),
              ),
            ],
          ),
          const SizedBox(height: 10),
          Row(
            children: [
              Expanded(
                child: _buildSummaryMetric('Pending', '$pending'),
              ),
              const SizedBox(width: 10),
              Expanded(
                child: _buildSummaryMetric('Orders', '${_allOrders.length}'),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildSummaryMetric(String title, String value) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
      decoration: BoxDecoration(
        color: Colors.white.withValues(alpha: 0.16),
        borderRadius: BorderRadius.circular(14),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(title,
              style: const TextStyle(fontSize: 11, color: Colors.white70)),
          const SizedBox(height: 4),
          Text(value,
              style: const TextStyle(
                  fontSize: 15,
                  fontWeight: FontWeight.bold,
                  color: Colors.white)),
        ],
      ),
    );
  }

  Widget _buildFilterChip(String label, int statusValue) {
    final isSelected = _selectedStatusFilter == statusValue;
    return Padding(
      padding: const EdgeInsets.only(right: 8.0),
      child: FilterChip(
        selected: isSelected,
        label: Text(
          label,
          style: TextStyle(
            color: isSelected ? Colors.white : AppColors.textPrimary,
            fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
            fontSize: 12,
          ),
        ),
        backgroundColor: Colors.grey.shade100,
        selectedColor: AppColors.primary,
        showCheckmark: false,
        onSelected: (selected) {
          setState(() {
            _selectedStatusFilter = statusValue;
          });
        },
      ),
    );
  }

  Widget _buildOrderCard(OrderModel order) {
    final itemPreview = order.orderItems.take(2).toList();

    return Container(
      margin: const EdgeInsets.only(bottom: 16),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: Colors.grey.shade200),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.03),
            blurRadius: 10,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        const Icon(Icons.receipt_long,
                            size: 16, color: AppColors.primary),
                        const SizedBox(width: 6),
                        Expanded(
                          child: Text(
                            '#${order.orderId.substring(0, 8).toUpperCase()}',
                            style: const TextStyle(
                                fontWeight: FontWeight.bold,
                                fontSize: 15,
                                color: AppColors.textPrimary),
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 2),
                    Text(
                      _formatDate(order.orderDate),
                      style: const TextStyle(
                          fontSize: 11, color: AppColors.textSecondary),
                    ),
                  ],
                ),
              ),
              Container(
                padding:
                    const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                decoration: BoxDecoration(
                  color: _getStatusBgColor(order.status),
                  borderRadius: BorderRadius.circular(100),
                ),
                child: Text(
                  order.statusText,
                  style: TextStyle(
                    fontSize: 11,
                    fontWeight: FontWeight.bold,
                    color: _getStatusColor(order.status),
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Container(
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: Colors.grey.shade50,
              borderRadius: BorderRadius.circular(12),
              border: Border.all(color: Colors.grey.shade200),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    const Icon(Icons.location_on_outlined,
                        size: 16, color: AppColors.primary),
                    const SizedBox(width: 6),
                    const Expanded(
                      child: Text(
                        'Delivery address',
                        style: TextStyle(
                            fontSize: 12,
                            fontWeight: FontWeight.bold,
                            color: AppColors.textSecondary),
                      ),
                    ),
                    if (order.phoneNumber.isNotEmpty)
                      Text(
                        order.phoneNumber,
                        style: const TextStyle(
                            fontSize: 11, color: AppColors.textSecondary),
                      ),
                  ],
                ),
                const SizedBox(height: 6),
                Text(
                  order.customerName.isNotEmpty
                      ? order.customerName
                      : 'Customer',
                  style: const TextStyle(
                      fontSize: 13,
                      fontWeight: FontWeight.bold,
                      color: AppColors.textPrimary),
                ),
                const SizedBox(height: 2),
                Text(
                  _getShortAddress(order.addressSnapshot),
                  style: const TextStyle(
                      fontSize: 12,
                      color: AppColors.textSecondary,
                      height: 1.35),
                ),
                const SizedBox(height: 10),
                Wrap(
                  spacing: 8,
                  runSpacing: 8,
                  children: [
                    _buildSummaryChip(
                        Icons.credit_card_outlined, order.paymentMethod),
                    _buildSummaryChip(Icons.payments_outlined,
                        _getPaymentStatusText(order.paymentStatus)),
                  ],
                ),
                if (itemPreview.isNotEmpty) ...[
                  const SizedBox(height: 10),
                  Wrap(
                    spacing: 8,
                    runSpacing: 8,
                    children: [
                      ...itemPreview.map((item) => Container(
                            padding: const EdgeInsets.symmetric(
                                horizontal: 10, vertical: 6),
                            decoration: BoxDecoration(
                              color: Colors.white,
                              borderRadius: BorderRadius.circular(999),
                              border: Border.all(color: Colors.grey.shade200),
                            ),
                            child: Text(
                              item.productName,
                              style: const TextStyle(
                                  fontSize: 11, color: AppColors.textPrimary),
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                          )),
                      if (order.orderItems.length > 2)
                        Container(
                          padding: const EdgeInsets.symmetric(
                              horizontal: 10, vertical: 6),
                          decoration: BoxDecoration(
                            color: AppColors.primary.withValues(alpha: 0.08),
                            borderRadius: BorderRadius.circular(999),
                          ),
                          child: Text(
                            '+${order.orderItems.length - 2} more',
                            style: const TextStyle(
                                fontSize: 11,
                                color: AppColors.primary,
                                fontWeight: FontWeight.bold),
                          ),
                        ),
                    ],
                  ),
                ],
              ],
            ),
          ),
          const SizedBox(height: 12),
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
            decoration: BoxDecoration(
              color: AppColors.primary.withValues(alpha: 0.05),
              borderRadius: BorderRadius.circular(12),
            ),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text('Total',
                        style: TextStyle(
                            fontSize: 12, color: AppColors.textSecondary)),
                    const SizedBox(height: 2),
                    Text(
                      _formatCurrency(order.totalAmount),
                      style: const TextStyle(
                          fontSize: 17,
                          fontWeight: FontWeight.bold,
                          color: AppColors.primary),
                    ),
                  ],
                ),
                if (order.discountAmount != null && order.discountAmount! > 0)
                  Container(
                    padding:
                        const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                    decoration: BoxDecoration(
                      color: Colors.green.withValues(alpha: 0.12),
                      borderRadius: BorderRadius.circular(999),
                    ),
                    child: Text(
                      'Discount ${order.discountAmount!.toStringAsFixed(0)}đ',
                      style: const TextStyle(
                          fontSize: 12,
                          color: Colors.green,
                          fontWeight: FontWeight.bold),
                    ),
                  ),
              ],
            ),
          ),
          const SizedBox(height: 14),
          Row(
            mainAxisAlignment: MainAxisAlignment.end,
            children: [
              if (order.status == 1 || order.status == 2) ...[
                OutlinedButton(
                  style: OutlinedButton.styleFrom(
                    foregroundColor: Colors.red,
                    side: const BorderSide(color: Colors.red),
                    shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(8)),
                    padding:
                        const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                  ),
                  onPressed: () => _handleCancelOrder(order),
                  child: const Text('Cancel',
                      style:
                          TextStyle(fontSize: 12, fontWeight: FontWeight.bold)),
                ),
                const SizedBox(width: 8),
              ],
              ElevatedButton(
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.primary,
                  foregroundColor: Colors.white,
                  shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(8)),
                  padding:
                      const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
                ),
                onPressed: () => _showOrderDetailsModal(order),
                child: const Text('View Details',
                    style:
                        TextStyle(fontSize: 12, fontWeight: FontWeight.bold)),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildSummaryChip(IconData icon, String label) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(999),
        border: Border.all(color: Colors.grey.shade200),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 14, color: AppColors.primary),
          const SizedBox(width: 6),
          Text(label,
              style:
                  const TextStyle(fontSize: 11, color: AppColors.textPrimary)),
        ],
      ),
    );
  }

  Widget _buildModalSummaryChip(IconData icon, String label,
      {Color? badgeColor}) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(999),
        border: Border.all(color: Colors.grey.shade200),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 14, color: badgeColor ?? AppColors.primary),
          const SizedBox(width: 6),
          Text(label,
              style:
                  const TextStyle(fontSize: 11, color: AppColors.textPrimary)),
        ],
      ),
    );
  }
}
