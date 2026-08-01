import 'dart:io';

import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import '../../models/ai_result_model.dart';
import '../../services/api_service.dart';

class ClassifyAIPage extends StatefulWidget {
  const ClassifyAIPage({super.key});

  @override
  State<ClassifyAIPage> createState() =>
      _ClassifyAIPageState();
}

class _ClassifyAIPageState
    extends State<ClassifyAIPage> {
  final ApiService _apiService = ApiService();
  final ImagePicker _picker = ImagePicker();

  File? _selectedImage;
  AIResultModel? _result;

  bool _isLoading = false;
  String? _errorMessage;

  // =========================
  // PICK IMAGE
  // =========================

  Future<void> _pickImage(
      ImageSource source,
      ) async {
    try {
      final XFile? pickedFile =
      await _picker.pickImage(
        source: source,
        imageQuality: 90,
      );

      if (pickedFile == null) return;

      setState(() {
        _selectedImage =
            File(pickedFile.path);

        _result = null;
        _errorMessage = null;
      });
    } catch (e) {
      setState(() {
        _errorMessage =
        'Unable to select image.';
      });
    }
  }

  // =========================
  // SHOW IMAGE SOURCE
  // =========================

  Future<void> _showImageSourceDialog() async {
    showModalBottomSheet(
      context: context,
      builder: (context) {
        return SafeArea(
          child: Wrap(
            children: [
              ListTile(
                leading: const Icon(
                  Icons.camera_alt_outlined,
                ),
                title: const Text(
                  'Take a photo',
                ),
                onTap: () {
                  Navigator.pop(context);

                  _pickImage(
                    ImageSource.camera,
                  );
                },
              ),
              ListTile(
                leading: const Icon(
                  Icons.photo_library_outlined,
                ),
                title: const Text(
                  'Choose from gallery',
                ),
                onTap: () {
                  Navigator.pop(context);

                  _pickImage(
                    ImageSource.gallery,
                  );
                },
              ),
            ],
          ),
        );
      },
    );
  }

  // =========================
  // CLASSIFY AI
  // =========================

  Future<void> _classifyImage() async {
    if (_selectedImage == null) {
      setState(() {
        _errorMessage =
        'Please select an image first.';
      });

      return;
    }

    setState(() {
      _isLoading = true;
      _errorMessage = null;
      _result = null;
    });

    try {
      final result =
      await _apiService.classifyAI(
        _selectedImage!,
      );

      if (!mounted) return;

      setState(() {
        _result = result;
        _isLoading = false;
      });
    } catch (e) {
      if (!mounted) return;

      setState(() {
        _isLoading = false;
        _errorMessage =
        'Unable to classify image. Please try again.';
      });
    }
  }

  // =========================
  // CLEAR
  // =========================

  void _clearImage() {
    setState(() {
      _selectedImage = null;
      _result = null;
      _errorMessage = null;
    });
  }

  // =========================
  // UI
  // =========================

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text(
          'AI Pet Disease Diagnosis',
        ),
        centerTitle: true,
      ),

      body: SafeArea(
        child: SingleChildScrollView(
          padding:
          const EdgeInsets.all(20),

          child: Column(
            crossAxisAlignment:
            CrossAxisAlignment.stretch,

            children: [
              // =====================
              // HEADER
              // =====================

              const Icon(
                Icons
                    .medical_services_outlined,
                size: 64,
              ),

              const SizedBox(height: 12),

              const Text(
                'AI Pet Disease Diagnosis',
                textAlign: TextAlign.center,
                style: TextStyle(
                  fontSize: 24,
                  fontWeight:
                  FontWeight.bold,
                ),
              ),

              const SizedBox(height: 8),

              Text(
                'Upload a photo of your pet\'s skin '
                    'to let AI analyze possible diseases.',
                textAlign: TextAlign.center,
                style: TextStyle(
                  fontSize: 15,
                  color:
                  Colors.grey.shade600,
                ),
              ),

              const SizedBox(height: 24),

              // =====================
              // IMAGE PREVIEW
              // =====================

              GestureDetector(
                onTap:
                _showImageSourceDialog,

                child: Container(
                  height: 300,

                  decoration:
                  BoxDecoration(
                    borderRadius:
                    BorderRadius.circular(
                      16,
                    ),

                    border: Border.all(
                      color: Colors
                          .grey
                          .shade300,
                    ),
                  ),

                  child:
                  _selectedImage != null
                      ? ClipRRect(
                    borderRadius:
                    BorderRadius
                        .circular(
                      16,
                    ),

                    child:
                    Image.file(
                      _selectedImage!,
                      width:
                      double.infinity,
                      height:
                      double.infinity,
                      fit: BoxFit
                          .cover,
                    ),
                  )
                      : Column(
                    mainAxisAlignment:
                    MainAxisAlignment
                        .center,

                    children: [
                      Icon(
                        Icons
                            .cloud_upload_outlined,
                        size: 64,
                        color: Colors
                            .grey
                            .shade500,
                      ),

                      const SizedBox(
                        height: 12,
                      ),

                      const Text(
                        'Select an image',
                        style:
                        TextStyle(
                          fontSize:
                          18,
                          fontWeight:
                          FontWeight
                              .w600,
                        ),
                      ),

                      const SizedBox(
                        height: 6,
                      ),

                      Text(
                        'Tap here to choose '
                            'a photo from gallery '
                            'or camera.',
                        textAlign:
                        TextAlign
                            .center,
                        style:
                        TextStyle(
                          color: Colors
                              .grey
                              .shade600,
                        ),
                      ),
                    ],
                  ),
                ),
              ),

              const SizedBox(height: 16),

              // =====================
              // SELECT IMAGE BUTTON
              // =====================

              OutlinedButton.icon(
                onPressed:
                _showImageSourceDialog,

                icon: const Icon(
                  Icons
                      .add_photo_alternate_outlined,
                ),

                label: Text(
                  _selectedImage == null
                      ? 'Select Image'
                      : 'Change Image',
                ),

                style:
                OutlinedButton.styleFrom(
                  padding:
                  const EdgeInsets
                      .symmetric(
                    vertical: 14,
                  ),

                  shape:
                  RoundedRectangleBorder(
                    borderRadius:
                    BorderRadius.circular(
                      12,
                    ),
                  ),
                ),
              ),

              const SizedBox(height: 12),

              // =====================
              // CLEAR BUTTON
              // =====================

              if (_selectedImage !=
                  null)
                TextButton.icon(
                  onPressed:
                  _clearImage,

                  icon: const Icon(
                    Icons.delete_outline,
                  ),

                  label: const Text(
                    'Remove Image',
                  ),
                ),

              const SizedBox(height: 12),

              // =====================
              // ERROR
              // =====================

              if (_errorMessage != null)
                Container(
                  padding:
                  const EdgeInsets.all(
                    12,
                  ),

                  decoration:
                  BoxDecoration(
                    color: Colors.red.withAlpha(20),

                    borderRadius:
                    BorderRadius.circular(
                      10,
                    ),
                  ),

                  child: Row(
                    children: [
                      const Icon(
                        Icons
                            .error_outline,
                        color: Colors.red,
                      ),

                      const SizedBox(
                        width: 8,
                      ),

                      Expanded(
                        child: Text(
                          _errorMessage!,
                          style:
                          const TextStyle(
                            color:
                            Colors.red,
                          ),
                        ),
                      ),
                    ],
                  ),
                ),

              const SizedBox(height: 16),

              // =====================
              // ANALYZE BUTTON
              // =====================

              SizedBox(
                height: 52,

                child: ElevatedButton.icon(
                  onPressed:
                  _isLoading ||
                      _selectedImage ==
                          null
                      ? null
                      : _classifyImage,

                  icon: _isLoading
                      ? const SizedBox(
                    width: 20,
                    height: 20,
                    child:
                    CircularProgressIndicator(
                      strokeWidth: 2,
                      color:
                      Colors.white,
                    ),
                  )
                      : const Icon(
                    Icons
                        .auto_awesome,
                  ),

                  label: Text(
                    _isLoading
                        ? 'Analyzing...'
                        : 'Analyze with AI',
                  ),

                  style:
                  ElevatedButton
                      .styleFrom(
                    shape:
                    RoundedRectangleBorder(
                      borderRadius:
                      BorderRadius
                          .circular(
                        12,
                      ),
                    ),
                  ),
                ),
              ),

              // =====================
              // RESULT
              // =====================

              if (_result != null)
                _buildResult(
                  _result!,
                ),
            ],
          ),
        ),
      ),
    );
  }

  // =========================
  // RESULT WIDGET
  // =========================

  Widget _buildResult(
      AIResultModel result,
      ) {
    final confidence =
        result.confidence;

    return Container(
      margin:
      const EdgeInsets.only(
        top: 24,
      ),

      padding:
      const EdgeInsets.all(20),

      decoration:
      BoxDecoration(
        borderRadius:
        BorderRadius.circular(
          16,
        ),

        border: Border.all(
          color:
          Colors.grey.shade300,
        ),
      ),

      child: Column(
        crossAxisAlignment:
        CrossAxisAlignment.start,

        children: [
          const Text(
            'AI Analysis Result',
            style: TextStyle(
              fontSize: 20,
              fontWeight:
              FontWeight.bold,
            ),
          ),

          const SizedBox(height: 20),

          // =====================
          // IMAGE VALIDATION
          // =====================

          if (!result.isDiseaseImage)
            Container(
              padding:
              const EdgeInsets.all(
                14,
              ),

              decoration:
              BoxDecoration(
                borderRadius:
                BorderRadius.circular(
                  10,
                ),
              ),

              child: Row(
                children: [
                  const Icon(
                    Icons.info_outline,
                  ),

                  const SizedBox(
                    width: 10,
                  ),

                  Expanded(
                    child: Text(
                      result.message ??
                          'The uploaded image could not be identified as a suitable disease image.',
                    ),
                  ),
                ],
              ),
            )

          else ...[
            // =====================
            // DISEASE NAME
            // =====================

            Text(
              result.diseaseName,
              style: const TextStyle(
                fontSize: 24,
                fontWeight:
                FontWeight.bold,
              ),
            ),

            const SizedBox(height: 16),

            // =====================
            // CONFIDENCE
            // =====================

            Text(
              'Confidence: '
                  '${(confidence * 100).toStringAsFixed(1)}%',
              style:
              const TextStyle(
                fontSize: 16,
                fontWeight:
                FontWeight.w600,
              ),
            ),

            const SizedBox(height: 8),

            LinearProgressIndicator(
              value:
              confidence > 1
                  ? confidence / 100
                  : confidence,

              minHeight: 8,

              borderRadius:
              BorderRadius.circular(
                10,
              ),
            ),

            // =====================
            // DESCRIPTION
            // =====================

            if (result.description !=
                null &&
                result.description!
                    .isNotEmpty) ...[
              const SizedBox(
                height: 20,
              ),

              const Text(
                'Description',
                style: TextStyle(
                  fontSize: 17,
                  fontWeight:
                  FontWeight.bold,
                ),
              ),

              const SizedBox(
                height: 6,
              ),

              Text(
                result.description!,
                style:
                const TextStyle(
                  fontSize: 15,
                  height: 1.5,
                ),
              ),
            ],

            // =====================
            // RECOMMENDATION
            // =====================

            if (result.recommendation !=
                null &&
                result.recommendation!
                    .isNotEmpty) ...[
              const SizedBox(
                height: 20,
              ),

              const Text(
                'Recommendation',
                style: TextStyle(
                  fontSize: 17,
                  fontWeight:
                  FontWeight.bold,
                ),
              ),

              const SizedBox(
                height: 6,
              ),

              Text(
                result.recommendation!,
                style:
                const TextStyle(
                  fontSize: 15,
                  height: 1.5,
                ),
              ),
            ],
          ],
        ],
      ),
    );
  }
}