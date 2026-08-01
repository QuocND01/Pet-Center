import 'dart:io';
import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import '../../../constants/app_colors.dart';
import '../../../models/order_model.dart';
import '../../../models/product_feedback_model.dart';
import '../../../services/api_service.dart';

class EditOrderFeedbackSheet extends StatefulWidget {
  final ProductFeedbackModel feedback;
  final OrderItemModel item;

  const EditOrderFeedbackSheet({
    super.key,
    required this.feedback,
    required this.item,
  });

  @override
  State<EditOrderFeedbackSheet> createState() => _EditOrderFeedbackSheetState();
}

class _EditOrderFeedbackSheetState extends State<EditOrderFeedbackSheet> {
  final ApiService _apiService = ApiService();
  final ImagePicker _picker = ImagePicker();

  late int _rating;
  late String _comment;
  late List<FeedbackMediaModel> _currentMedia;
  final List<String> _removedPublicIds = [];
  final List<File> _newImageFiles = [];
  File? _newVideoFile;

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
    _rating = widget.feedback.rating;
    _comment = widget.feedback.comment ?? '';
    _currentMedia = List.from(widget.feedback.mediaFiles);
  }

  int get _currentImageCount =>
      _currentMedia.where((m) => m.mediaType.toLowerCase() == 'image').length + _newImageFiles.length;

  int get _currentVideoCount =>
      _currentMedia.where((m) => m.mediaType.toLowerCase() == 'video').length + (_newVideoFile != null ? 1 : 0);

  Future<void> _pickImage() async {
    if (_currentImageCount >= maxImages) {
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
        _newImageFiles.add(ioFile);
      });
    }
  }

  Future<void> _pickVideo() async {
    if (_currentVideoCount >= maxVideos) {
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
        _newVideoFile = ioFile;
      });
    }
  }

  void _removeExistingMedia(FeedbackMediaModel media) {
    setState(() {
      _currentMedia.remove(media);
      if (media.publicId != null && media.publicId!.isNotEmpty) {
        _removedPublicIds.add(media.publicId!);
      }
    });
  }

  void _handleSubmit() async {
    if (_comment.length > maxCommentLength) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Comment cannot exceed $maxCommentLength characters.'),
          backgroundColor: AppColors.error,
        ),
      );
      return;
    }

    setState(() {
      _isSubmitting = true;
    });

    try {
      final List<File> newMedia = [..._newImageFiles];
      if (_newVideoFile != null) {
        newMedia.add(_newVideoFile!);
      }

      final res = await _apiService.updateFeedback(
        feedbackId: widget.feedback.feedbackId,
        rating: _rating,
        comment: _comment,
        newMediaFiles: newMedia.isNotEmpty ? newMedia : null,
        removedPublicIds: _removedPublicIds.isNotEmpty ? _removedPublicIds : null,
      );

      if (!mounted) return;
      setState(() {
        _isSubmitting = false;
      });

      final isSuccess = res['success'] == true || res['Success'] == true;
      if (isSuccess) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Review updated successfully!'),
            backgroundColor: AppColors.success,
          ),
        );
        Navigator.pop(context, true); // return true to refresh
      } else {
        final msg = res['message'] ?? res['Message'] ?? 'Unable to update review. Please try again.';
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
    final ratingLabel = (_rating >= 1 && _rating <= 5) ? _starLabels[_rating] : '';

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

          // Header
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
                          color: Colors.indigo.shade50,
                          shape: BoxShape.circle,
                        ),
                        child: const Icon(Icons.edit_note_rounded, color: Colors.indigo, size: 24),
                      ),
                      const SizedBox(width: 12),
                      const Expanded(
                        child: Text(
                          'Edit Review',
                          style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: AppColors.textPrimary),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
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
          const Divider(height: 20),

          // Body Scrollable Form
          Expanded(
            child: SingleChildScrollView(
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
              child: Container(
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
                    // Product Header
                    Row(
                      children: [
                        ClipRRect(
                          borderRadius: BorderRadius.circular(8),
                          child: Container(
                            width: 48,
                            height: 48,
                            color: Colors.grey.shade100,
                            child: widget.item.productImage != null && widget.item.productImage!.isNotEmpty
                                ? Image.network(widget.item.productImage!, fit: BoxFit.cover)
                                : const Icon(Icons.pets, color: Colors.grey, size: 24),
                          ),
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: Text(
                            widget.item.productName,
                            style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14, color: AppColors.textPrimary),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                        ),
                      ],
                    ),
                    const Divider(height: 20),

                    // Rating Section
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
                            final isSelected = starVal <= _rating;
                            return GestureDetector(
                              onTap: () {
                                setState(() {
                                  _rating = starVal;
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

                    // Comment Section
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        const Text(
                          'DETAILED COMMENT',
                          style: TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: AppColors.textSecondary, letterSpacing: 0.5),
                        ),
                        Text(
                          '${_comment.length} / $maxCommentLength',
                          style: TextStyle(
                            fontSize: 11,
                            color: _comment.length > maxCommentLength ? AppColors.error : AppColors.textSecondary,
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 6),
                    TextFormField(
                      initialValue: _comment,
                      maxLines: 4,
                      maxLength: maxCommentLength,
                      buildCounter: (ctx, {required currentLength, required isFocused, maxLength}) => null,
                      onChanged: (val) {
                        setState(() {
                          _comment = val;
                        });
                      },
                      decoration: InputDecoration(
                        hintText: 'Share your experience...',
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

                    // Current Media Section (if any)
                    if (_currentMedia.isNotEmpty) ...[
                      const Text(
                        'CURRENT MEDIA',
                        style: TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: AppColors.textSecondary, letterSpacing: 0.5),
                      ),
                      const SizedBox(height: 8),
                      SizedBox(
                        height: 74,
                        child: ListView.separated(
                          scrollDirection: Axis.horizontal,
                          itemCount: _currentMedia.length,
                          separatorBuilder: (ctx, idx) => const SizedBox(width: 8),
                          itemBuilder: (ctx, idx) {
                            final media = _currentMedia[idx];
                            return Stack(
                              clipBehavior: Clip.none,
                              children: [
                                ClipRRect(
                                  borderRadius: BorderRadius.circular(8),
                                  child: Container(
                                    width: 74,
                                    height: 74,
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
                                ),
                                Positioned(
                                  top: -6,
                                  right: -6,
                                  child: GestureDetector(
                                    onTap: () => _removeExistingMedia(media),
                                    child: Container(
                                      decoration: const BoxDecoration(color: Colors.red, shape: BoxShape.circle),
                                      padding: const EdgeInsets.all(2),
                                      child: const Icon(Icons.close, color: Colors.white, size: 14),
                                    ),
                                  ),
                                ),
                              ],
                            );
                          },
                        ),
                      ),
                      const SizedBox(height: 16),
                    ],

                    // Add New Media Section
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        const Text(
                          'ADD NEW MEDIA',
                          style: TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: AppColors.textSecondary, letterSpacing: 0.5),
                        ),
                        Text(
                          'Max $maxImages photos + $maxVideos video',
                          style: const TextStyle(fontSize: 11, color: AppColors.textSecondary),
                        ),
                      ],
                    ),
                    const SizedBox(height: 8),

                    // Action Buttons: Add Photo & Add Video
                    Row(
                      children: [
                        OutlinedButton.icon(
                          style: OutlinedButton.styleFrom(
                            foregroundColor: AppColors.primary,
                            side: const BorderSide(color: AppColors.primary),
                            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                          ),
                          onPressed: _pickImage,
                          icon: const Icon(Icons.add_a_photo_outlined, size: 18),
                          label: Text('Add Photo ($_currentImageCount/$maxImages)'),
                        ),
                        const SizedBox(width: 8),
                        OutlinedButton.icon(
                          style: OutlinedButton.styleFrom(
                            foregroundColor: Colors.purple,
                            side: const BorderSide(color: Colors.purple),
                            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                          ),
                          onPressed: _pickVideo,
                          icon: const Icon(Icons.video_call_outlined, size: 18),
                          label: Text(_newVideoFile == null && _currentVideoCount == 0 ? 'Add Video' : 'Video Selected'),
                        ),
                      ],
                    ),

                    // New Media Previews
                    if (_newImageFiles.isNotEmpty || _newVideoFile != null) ...[
                      const SizedBox(height: 12),
                      SizedBox(
                        height: 74,
                        child: ListView(
                          scrollDirection: Axis.horizontal,
                          children: [
                            ..._newImageFiles.map((img) {
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
                                            _newImageFiles.remove(img);
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
                            if (_newVideoFile != null) ...[
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
                                            _newVideoFile = null;
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
              ),
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
                          'Save Changes',
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
}
