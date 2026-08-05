import 'package:flutter/material.dart';
import '../../../constants/app_colors.dart';
import '../../../models/order_model.dart';
import '../../../models/product_feedback_model.dart';
import '../../../services/api_service.dart';
import '../../../utils/app_error_utils.dart';
import 'edit_order_feedback_sheet.dart';

class ViewOrderFeedbackSheet extends StatefulWidget {
  final OrderModel order;

  const ViewOrderFeedbackSheet({super.key, required this.order});

  @override
  State<ViewOrderFeedbackSheet> createState() => _ViewOrderFeedbackSheetState();
}

class _ViewOrderFeedbackSheetState extends State<ViewOrderFeedbackSheet> {
  final ApiService _apiService = ApiService();
  late Future<List<ProductFeedbackModel>> _feedbacksFuture;

  final List<String> _starLabels = ['', 'Terrible', 'Poor', 'Average', 'Good', 'Excellent'];

  @override
  void initState() {
    super.initState();
    _feedbacksFuture = _apiService.getFeedbacksByOrderId(widget.order.orderId);
  }

  String _formatDate(DateTime? dt) {
    if (dt == null) return '';
    final d = dt.day.toString().padLeft(2, '0');
    final m = dt.month.toString().padLeft(2, '0');
    final y = dt.year;
    return '$d/$m/$y';
  }

  String _getInitials(String? name) {
    if (name == null || name.trim().isEmpty) return '?';
    final parts = name.trim().split(' ').where((w) => w.isNotEmpty).toList();
    if (parts.length == 1) return parts.first[0].toUpperCase();
    return (parts.first[0] + parts.last[0]).toUpperCase();
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      height: MediaQuery.of(context).size.height * 0.85,
      decoration: const BoxDecoration(
        color: AppColors.background,
        borderRadius: BorderRadius.only(
          topLeft: Radius.circular(24),
          topRight: Radius.circular(24),
        ),
      ),
      child: Column(
        children: [
          // Drag handle
          const SizedBox(height: 12),
          Container(
            width: 40,
            height: 4,
            decoration: BoxDecoration(
              color: Colors.grey.shade300,
              borderRadius: BorderRadius.circular(2),
            ),
          ),
          const SizedBox(height: 12),

          // Sheet Header
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 20),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Expanded(
                  child: Row(
                    children: [
                      Container(
                        padding: const EdgeInsets.all(8),
                        decoration: BoxDecoration(
                          color: Colors.amber.shade50,
                          shape: BoxShape.circle,
                        ),
                        child: const Icon(Icons.star_rounded, color: Colors.amber, size: 24),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            const Text(
                              'Order Reviews',
                              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: AppColors.textPrimary),
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                            Text(
                              'Order #${widget.order.orderId.substring(0, 8).toUpperCase()}',
                              style: const TextStyle(fontSize: 12, color: AppColors.textSecondary),
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                ),
                IconButton(
                  icon: const Icon(Icons.close),
                  onPressed: () => Navigator.pop(context),
                ),
              ],
            ),
          ),
          const Divider(height: 24),

          // Body
          Expanded(
            child: FutureBuilder<List<ProductFeedbackModel>>(
              future: _feedbacksFuture,
              builder: (context, snapshot) {
                if (snapshot.connectionState == ConnectionState.waiting) {
                  return const Center(child: CircularProgressIndicator(color: AppColors.primary));
                }

                if (snapshot.hasError) {
                  return Center(
                    child: Padding(
                      padding: const EdgeInsets.all(24.0),
                      child: Text(
                        AppErrorUtils.getFriendlyMessage(snapshot.error),
                        textAlign: TextAlign.center,
                        style: const TextStyle(color: AppColors.error, fontSize: 13),
                      ),
                    ),
                  );
                }

                final list = snapshot.data ?? [];
                if (list.isEmpty) {
                  return const Center(
                    child: Text('No reviews submitted for this order.', style: TextStyle(color: AppColors.textSecondary)),
                  );
                }

                return ListView.separated(
                  padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                  itemCount: list.length,
                  separatorBuilder: (ctx, idx) => const SizedBox(height: 16),
                  itemBuilder: (ctx, idx) {
                    final feedback = list[idx];
                    final item = widget.order.orderItems.firstWhere(
                      (i) => i.productId.toLowerCase() == feedback.productId.toLowerCase(),
                      orElse: () => OrderItemModel(
                        productId: feedback.productId,
                        productName: 'Product',
                        quantity: 1,
                        unitPrice: 0,
                        subTotal: 0,
                      ),
                    );

                    return _buildFeedbackCard(feedback, item);
                  },
                );
              },
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildFeedbackCard(ProductFeedbackModel feedback, OrderItemModel item) {
    final ratingLabel = (feedback.rating >= 1 && feedback.rating <= 5) ? _starLabels[feedback.rating] : '';

    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: AppColors.inputBorder),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withAlpha(8),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Product header
          Row(
            children: [
              ClipRRect(
                borderRadius: BorderRadius.circular(8),
                child: Container(
                  width: 44,
                  height: 44,
                  color: Colors.grey.shade100,
                  child: item.productImage != null && item.productImage!.isNotEmpty
                      ? Image.network(item.productImage!, fit: BoxFit.cover)
                      : const Icon(Icons.pets, color: Colors.grey, size: 24),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      item.productName,
                      style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14, color: AppColors.textPrimary),
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                    const SizedBox(height: 2),
                    Row(
                      children: List.generate(
                        5,
                        (starIdx) => Icon(
                          starIdx < feedback.rating ? Icons.star_rounded : Icons.star_border_rounded,
                          color: starIdx < feedback.rating ? Colors.amber : Colors.grey.shade300,
                          size: 18,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                decoration: BoxDecoration(
                  color: Colors.amber.shade50,
                  borderRadius: BorderRadius.circular(100),
                ),
                child: Text(
                  ratingLabel,
                  style: TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: Colors.amber.shade900),
                ),
              ),
            ],
          ),
          const Divider(height: 20),

          // User Info & Date
          Row(
            children: [
              CircleAvatar(
                radius: 16,
                backgroundColor: AppColors.primary.withAlpha(30),
                child: Text(
                  _getInitials(feedback.customerName),
                  style: const TextStyle(fontSize: 12, fontWeight: FontWeight.bold, color: AppColors.primary),
                ),
              ),
              const SizedBox(width: 8),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      feedback.customerName ?? 'Customer',
                      style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13),
                    ),
                    if (feedback.createdDate != null)
                      Text(
                        _formatDate(feedback.createdDate),
                        style: const TextStyle(fontSize: 11, color: AppColors.textSecondary),
                      ),
                  ],
                ),
              ),
              InkWell(
                onTap: () async {
                  final refresh = await showModalBottomSheet<bool>(
                    context: context,
                    isScrollControlled: true,
                    backgroundColor: Colors.transparent,
                    builder: (ctx) => EditOrderFeedbackSheet(
                      feedback: feedback,
                      item: item,
                    ),
                  );
                  if (refresh == true) {
                    setState(() {
                      _feedbacksFuture = _apiService.getFeedbacksByOrderId(widget.order.orderId);
                    });
                  }
                },
                borderRadius: BorderRadius.circular(6),
                child: Container(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                  decoration: BoxDecoration(
                    border: Border.all(color: AppColors.primary.withAlpha(100), width: 1.2),
                    borderRadius: BorderRadius.circular(6),
                    color: Colors.indigo.shade50.withAlpha(50),
                  ),
                  child: const Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Icon(Icons.edit_outlined, size: 13, color: AppColors.primary),
                      SizedBox(width: 4),
                      Text(
                        'Edit',
                        style: TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: AppColors.primary),
                      ),
                    ],
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),

          // Comment
          Container(
            width: double.infinity,
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: Colors.grey.shade50,
              borderRadius: BorderRadius.circular(10),
            ),
            child: Text(
              (feedback.comment != null && feedback.comment!.trim().isNotEmpty)
                  ? feedback.comment!
                  : 'No detailed comment provided.',
              style: TextStyle(
                fontSize: 13,
                color: (feedback.comment != null && feedback.comment!.trim().isNotEmpty)
                    ? AppColors.textPrimary
                    : AppColors.textSecondary,
                fontStyle: (feedback.comment != null && feedback.comment!.trim().isNotEmpty)
                    ? FontStyle.normal
                    : FontStyle.italic,
                height: 1.4,
              ),
            ),
          ),

          // Media Files Grid (if any)
          if (feedback.mediaFiles.isNotEmpty) ...[
            const SizedBox(height: 12),
            SizedBox(
              height: 72,
              child: ListView.separated(
                scrollDirection: Axis.horizontal,
                itemCount: feedback.mediaFiles.length,
                separatorBuilder: (ctx, idx) => const SizedBox(width: 8),
                itemBuilder: (ctx, idx) {
                  final media = feedback.mediaFiles[idx];
                  return ClipRRect(
                    borderRadius: BorderRadius.circular(8),
                    child: Container(
                      width: 72,
                      height: 72,
                      color: Colors.black12,
                      child: Stack(
                        fit: StackFit.expand,
                        children: [
                          Image.network(
                            media.mediaUrl,
                            fit: BoxFit.cover,
                            errorBuilder: (ctx, err, st) => const Icon(Icons.broken_image, color: Colors.grey),
                          ),
                          if (media.mediaType.toLowerCase() == 'video')
                            Container(
                              color: Colors.black26,
                              child: const Center(
                                child: Icon(Icons.play_circle_fill, color: Colors.white, size: 28),
                              ),
                            ),
                        ],
                      ),
                    ),
                  );
                },
              ),
            ),
          ],

          // Shop Reply (if available)
          if (feedback.reply != null && feedback.reply!.trim().isNotEmpty) ...[
            const SizedBox(height: 12),
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: const Color(0xFFF0FDF4),
                borderRadius: BorderRadius.circular(10),
                border: const Border(left: BorderSide(color: Color(0xFF22C55E), width: 3)),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      const Icon(Icons.storefront_rounded, color: Color(0xFF16A34A), size: 16),
                      const SizedBox(width: 6),
                      const Text(
                        'Shop Reply',
                        style: TextStyle(fontSize: 12, fontWeight: FontWeight.bold, color: Color(0xFF16A34A)),
                      ),
                      if (feedback.replyDate != null) ...[
                        const Text(' · ', style: TextStyle(color: Colors.grey)),
                        Text(
                          _formatDate(feedback.replyDate),
                          style: const TextStyle(fontSize: 11, color: AppColors.textSecondary),
                        ),
                      ],
                    ],
                  ),
                  const SizedBox(height: 6),
                  Text(
                    feedback.reply!,
                    style: const TextStyle(fontSize: 13, color: Color(0xFF15803D), height: 1.4),
                  ),
                ],
              ),
            ),
          ],
        ],
      ),
    );
  }
}
