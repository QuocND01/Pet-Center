import 'dart:io';
import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import '../../../constants/app_colors.dart';
import '../../../models/order_model.dart';
import '../../../models/order_feedback_input.dart';
import '../../../services/api_service.dart';

class WriteOrderFeedbackSheet extends StatefulWidget {
  final OrderModel order;

  const WriteOrderFeedbackSheet({super.key, required this.order});

  @override
  State<WriteOrderFeedbackSheet> createState() => _WriteOrderFeedbackSheetState();
}

class _WriteOrderFeedbackSheetState extends State<WriteOrderFeedbackSheet> {
  final ApiService _apiService = ApiService();
  final ImagePicker _picker = ImagePicker();

  late List<ProductFeedbackInput> _feedbackDrafts;
  bool _isSubmitting = false;

  static const int maxImages = 2;
  static const int maxVideos = 1;
  static const int maxImageSizeBytes = 5 * 1024 * 1024; // 5MB
  static const int maxVideoSizeBytes = 30 * 1024 * 1024; // 30MB
  static const int maxCommentLength = 1000;

  final List<String> _starLabels = ['', 'Terrible', 'Poor', 'Average', 'Good', 'Excellent'];

  @override
  void initState() {
    super.initState();
    _feedbackDrafts = widget.order.orderItems.map((item) {
      return ProductFeedbackInput(
        productId: item.productId,
        productName: item.productName,
        productImage: item.productImage,
        orderId: widget.order.orderId,
        rating: 5,
        comment: '',
      );
    }).toList();
  }

  Future<void> _pickImage(ProductFeedbackInput draft) async {
    if (draft.imageFiles.length >= maxImages) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Maximum $maxImages photos allowed per product.'),
          backgroundColor: AppColors.error,
        ),
      );
      return;
    }

    final XFile? file = await _picker.pickImage(
      source: ImageSource.gallery,
      imageQuality: 85,
    );

    if (file != null) {
      final ioFile = File(file.path);
      final size = await ioFile.length();
      if (size > maxImageSizeBytes) {
        if (!mounted) return;
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Photo size must not exceed 5MB.'),
            backgroundColor: AppColors.error,
          ),
        );
        return;
      }

      setState(() {
        draft.imageFiles.add(ioFile);
      });
    }
  }

  Future<void> _pickVideo(ProductFeedbackInput draft) async {
    if (draft.videoFile != null) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Maximum $maxVideos video allowed per product.'),
          backgroundColor: AppColors.error,
        ),
      );
      return;
    }

    final XFile? file = await _picker.pickVideo(
      source: ImageSource.gallery,
      maxDuration: const Duration(minutes: 3),
    );

    if (file != null) {
      final ioFile = File(file.path);
      final size = await ioFile.length();
      if (size > maxVideoSizeBytes) {
        if (!mounted) return;
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Video size must not exceed 30MB.'),
            backgroundColor: AppColors.error,
          ),
        );
        return;
      }

      setState(() {
        draft.videoFile = ioFile;
      });
    }
  }

  void _handleSubmit() async {
    // Validate comments max length
    for (var draft in _feedbackDrafts) {
      if (draft.comment.length > maxCommentLength) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Comment for "${draft.productName}" cannot exceed $maxCommentLength characters.'),
            backgroundColor: AppColors.error,
          ),
        );
        return;
      }
    }

    setState(() {
      _isSubmitting = true;
    });

    try {
      final res = await _apiService.createBulkFeedback(_feedbackDrafts);
      if (!mounted) return;
      setState(() {
        _isSubmitting = false;
      });

      final isSuccess = res['success'] == true || res['Success'] == true;
      if (isSuccess) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Thank you for submitting your review!'),
            backgroundColor: AppColors.success,
          ),
        );
        Navigator.pop(context, true); // return true to refresh
      } else {
        final msg = res['message'] ?? res['Message'] ?? 'Unable to submit review. Please try again.';
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(msg), backgroundColor: AppColors.error),
        );
      }
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _isSubmitting = false;
      });
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Error: $e'), backgroundColor: AppColors.error),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final count = _feedbackDrafts.length;
    final plural = count > 1 ? 's' : '';

    return Container(
      height: MediaQuery.of(context).size.height * 0.9,
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

          // Header
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 20),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Row(
                  children: [
                    Container(
                      padding: const EdgeInsets.all(8),
                      decoration: BoxDecoration(
                        color: Colors.amber.shade100,
                        shape: BoxShape.circle,
                      ),
                      child: const Icon(Icons.rate_review_rounded, color: Colors.amber, size: 24),
                    ),
                    const SizedBox(width: 12),
                    Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text(
                          'Write a Review',
                          style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: AppColors.textPrimary),
                        ),
                        Text(
                          'Order #${widget.order.orderId.substring(0, 8).toUpperCase()}',
                          style: const TextStyle(fontSize: 12, color: AppColors.textSecondary),
                        ),
                      ],
                    ),
                  ],
                ),
                IconButton(
                  icon: const Icon(Icons.close),
                  onPressed: () => Navigator.pop(context),
                ),
              ],
            ),
          ),
          const Divider(height: 20),

          // Info Banner
          Container(
            width: double.infinity,
            margin: const EdgeInsets.symmetric(horizontal: 16),
            padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
            decoration: BoxDecoration(
              color: Colors.amber.shade50,
              borderRadius: BorderRadius.circular(10),
              border: Border.all(color: Colors.amber.shade200),
            ),
            child: Row(
              children: [
                Icon(Icons.info_outline, color: Colors.amber.shade900, size: 20),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    'Please rate all $count product$plural in this order. Your feedback helps other customers!',
                    style: TextStyle(fontSize: 12, color: Colors.amber.shade900, fontWeight: FontWeight.w600),
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: 12),

          // Drafts Scrollable List
          Expanded(
            child: ListView.separated(
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
              itemCount: _feedbackDrafts.length,
              separatorBuilder: (ctx, idx) => const SizedBox(height: 16),
              itemBuilder: (ctx, idx) {
                final draft = _feedbackDrafts[idx];
                return _buildProductDraftCard(draft, idx + 1, _feedbackDrafts.length);
              },
            ),
          ),

          // Submit Action Bar
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: Colors.white,
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withAlpha(10),
                  blurRadius: 10,
                  offset: const Offset(0, -4),
                ),
              ],
            ),
            child: SafeArea(
              child: SizedBox(
                width: double.infinity,
                height: 48,
                child: ElevatedButton(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppColors.primary,
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                    elevation: 2,
                  ),
                  onPressed: _isSubmitting ? null : _handleSubmit,
                  child: _isSubmitting
                      ? const SizedBox(
                          width: 24,
                          height: 24,
                          child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2.5),
                        )
                      : const Text(
                          'Submit Review',
                          style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.white),
                        ),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildProductDraftCard(ProductFeedbackInput draft, int index, int total) {
    final ratingLabel = (draft.rating >= 1 && draft.rating <= 5) ? _starLabels[draft.rating] : '';

    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: AppColors.inputBorder),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withAlpha(6),
            blurRadius: 6,
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
                  width: 48,
                  height: 48,
                  color: Colors.grey.shade100,
                  child: draft.productImage != null && draft.productImage!.isNotEmpty
                      ? Image.network(draft.productImage!, fit: BoxFit.cover)
                      : const Icon(Icons.pets, color: Colors.grey, size: 24),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      draft.productName,
                      style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14, color: AppColors.textPrimary),
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                    const SizedBox(height: 2),
                    Text(
                      'Product $index of $total',
                      style: const TextStyle(fontSize: 11, color: AppColors.textSecondary),
                    ),
                  ],
                ),
              ),
            ],
          ),
          const Divider(height: 20),

          // Rating Stars Bar
          const Text(
            'PRODUCT QUALITY',
            style: TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: AppColors.textSecondary, letterSpacing: 0.5),
          ),
          const SizedBox(height: 8),
          Row(
            children: [
              Row(
                children: List.generate(5, (starIdx) {
                  final starVal = starIdx + 1;
                  final isSelected = starVal <= draft.rating;
                  return GestureDetector(
                    onTap: () {
                      setState(() {
                        draft.rating = starVal;
                      });
                    },
                    child: Padding(
                      padding: const EdgeInsets.only(right: 6.0),
                      child: Icon(
                        isSelected ? Icons.star_rounded : Icons.star_border_rounded,
                        color: isSelected ? Colors.amber : Colors.grey.shade300,
                        size: 32,
                      ),
                    ),
                  );
                }),
              ),
              const SizedBox(width: 8),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                decoration: BoxDecoration(
                  color: Colors.amber.shade50,
                  borderRadius: BorderRadius.circular(100),
                ),
                child: Text(
                  ratingLabel,
                  style: TextStyle(fontSize: 12, fontWeight: FontWeight.bold, color: Colors.amber.shade900),
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),

          // Comment Text Field
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              const Text(
                'DETAILED COMMENT',
                style: TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: AppColors.textSecondary, letterSpacing: 0.5),
              ),
              Text(
                '${draft.comment.length} / $maxCommentLength',
                style: TextStyle(
                  fontSize: 11,
                  color: draft.comment.length > maxCommentLength ? AppColors.error : AppColors.textSecondary,
                ),
              ),
            ],
          ),
          const SizedBox(height: 6),
          TextFormField(
            initialValue: draft.comment,
            maxLines: 3,
            maxLength: maxCommentLength,
            buildCounter: (ctx, {required currentLength, required isFocused, maxLength}) => null,
            onChanged: (val) {
              setState(() {
                draft.comment = val;
              });
            },
            decoration: InputDecoration(
              hintText: 'Share your experience with this product...',
              hintStyle: const TextStyle(fontSize: 13, color: Colors.grey),
              border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
              enabledBorder: OutlineInputBorder(
                borderRadius: BorderRadius.circular(12),
                borderSide: const BorderSide(color: AppColors.inputBorder),
              ),
              focusedBorder: OutlineInputBorder(
                borderRadius: BorderRadius.circular(12),
                borderSide: const BorderSide(color: AppColors.primary, width: 2),
              ),
              filled: true,
              fillColor: Colors.grey.shade50,
            ),
          ),
          const SizedBox(height: 16),

          // Media Upload Section
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              const Text(
                'PHOTOS & VIDEOS',
                style: TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: AppColors.textSecondary, letterSpacing: 0.5),
              ),
              Text(
                'Max $maxImages photos + $maxVideos video',
                style: const TextStyle(fontSize: 11, color: AppColors.textSecondary),
              ),
            ],
          ),
          const SizedBox(height: 8),

          // Action Buttons: Add Image & Add Video
          Row(
            children: [
              OutlinedButton.icon(
                style: OutlinedButton.styleFrom(
                  foregroundColor: AppColors.primary,
                  side: const BorderSide(color: AppColors.primary),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                  padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                ),
                onPressed: () => _pickImage(draft),
                icon: const Icon(Icons.add_a_photo_outlined, size: 18),
                label: Text('Add Photo (${draft.imageFiles.length}/$maxImages)'),
              ),
              const SizedBox(width: 8),
              OutlinedButton.icon(
                style: OutlinedButton.styleFrom(
                  foregroundColor: Colors.purple,
                  side: const BorderSide(color: Colors.purple),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                  padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                ),
                onPressed: () => _pickVideo(draft),
                icon: const Icon(Icons.video_call_outlined, size: 18),
                label: Text(draft.videoFile == null ? 'Add Video' : 'Video Selected'),
              ),
            ],
          ),

          // Selected Media Previews Grid
          if (draft.imageFiles.isNotEmpty || draft.videoFile != null) ...[
            const SizedBox(height: 12),
            SizedBox(
              height: 74,
              child: ListView(
                scrollDirection: Axis.horizontal,
                children: [
                  // Images
                  ...draft.imageFiles.map((img) {
                    return Padding(
                      padding: const EdgeInsets.only(right: 8.0),
                      child: Stack(
                        clipBehavior: Clip.none,
                        children: [
                          ClipRRect(
                            borderRadius: BorderRadius.circular(8),
                            child: Image.file(img, width: 74, height: 74, fit: BoxFit.cover),
                          ),
                          Positioned(
                            top: -6,
                            right: -6,
                            child: GestureDetector(
                              onTap: () {
                                setState(() {
                                  draft.imageFiles.remove(img);
                                });
                              },
                              child: Container(
                                decoration: const BoxDecoration(color: Colors.red, shape: BoxShape.circle),
                                padding: const EdgeInsets.all(2),
                                child: const Icon(Icons.close, color: Colors.white, size: 14),
                              ),
                            ),
                          ),
                        ],
                      ),
                    );
                  }),

                  // Video
                  if (draft.videoFile != null) ...[
                    Padding(
                      padding: const EdgeInsets.only(right: 8.0),
                      child: Stack(
                        clipBehavior: Clip.none,
                        children: [
                          Container(
                            width: 74,
                            height: 74,
                            decoration: BoxDecoration(
                              color: Colors.black87,
                              borderRadius: BorderRadius.circular(8),
                            ),
                            child: const Column(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                Icon(Icons.play_circle_fill, color: Colors.white, size: 28),
                                SizedBox(height: 2),
                                Text('Video', style: TextStyle(color: Colors.white, fontSize: 10)),
                              ],
                            ),
                          ),
                          Positioned(
                            top: -6,
                            right: -6,
                            child: GestureDetector(
                              onTap: () {
                                setState(() {
                                  draft.videoFile = null;
                                });
                              },
                              child: Container(
                                decoration: const BoxDecoration(color: Colors.red, shape: BoxShape.circle),
                                padding: const EdgeInsets.all(2),
                                child: const Icon(Icons.close, color: Colors.white, size: 14),
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ],
              ),
            ),
          ],
        ],
      ),
    );
  }
}
