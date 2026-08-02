import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../../constants/app_colors.dart';
import '../../../models/appointment_model.dart';
import '../../../services/api_service.dart';
import '../../../widgets/custom_button.dart';
import 'appointment_update_screen.dart';

class AppointmentDetailScreen extends StatefulWidget {
  final String appointmentId;

  const AppointmentDetailScreen({
    super.key,
    required this.appointmentId,
  });

  @override
  State<AppointmentDetailScreen> createState() => _AppointmentDetailScreenState();
}

class _AppointmentDetailScreenState extends State<AppointmentDetailScreen> {
  final ApiService _apiService = ApiService();
  late Future<AppointmentDetailModel> _detailFuture;
  AppointmentDetailModel? _detail;
  bool _isActionLoading = false;

  @override
  void initState() {
    super.initState();
    _loadDetail();
  }

  void _loadDetail() {
    setState(() {
      _detailFuture = _apiService.getAppointmentDetail(widget.appointmentId).then((data) {
        _detail = data;
        return data;
      });
    });
  }

  Color _getStatusColor(int status) {
    switch (status) {
      case 1:
        return Colors.orange;
      case 2:
        return Colors.blue;
      case 3:
        return Colors.green;
      case 4:
        return Colors.red;
      default:
        return Colors.grey;
    }
  }

  void _cancelAppointment() {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Cancel Appointment'),
        content: const Text('Are you sure you want to cancel this appointment?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('No'),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
            onPressed: () async {
              Navigator.pop(ctx);
              setState(() {
                _isActionLoading = true;
              });
              try {
                final ok = await _apiService.cancelAppointment(widget.appointmentId);
                if (ok) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text('Appointment cancelled successfully.')),
                  );
                  _loadDetail();
                } else {
                  _showError('Failed to cancel appointment.');
                }
              } catch (e) {
                _showError('Cancel error: $e');
              } finally {
                setState(() {
                  _isActionLoading = false;
                });
              }
            },
            child: const Text('Yes, Cancel', style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );
  }

  void _showPaymentModal() {
    String selectedMethod = 'VNPAY';

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
                  const Text('Select Online Payment', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                  const Divider(),
                  RadioListTile<String>(
                    title: const Text('VNPAY Gateway'),
                    subtitle: const Text('Pay securely via VNPAY QR / Banking'),
                    secondary: const Icon(Icons.account_balance_wallet, color: Colors.blue),
                    value: 'VNPAY',
                    groupValue: selectedMethod,
                    onChanged: (val) {
                      setModalState(() {
                        selectedMethod = val!;
                      });
                    },
                  ),
                  RadioListTile<String>(
                    title: const Text('MoMo E-Wallet'),
                    subtitle: const Text('Pay with MoMo Wallet'),
                    secondary: const Icon(Icons.qr_code, color: Colors.pink),
                    value: 'MOMO',
                    groupValue: selectedMethod,
                    onChanged: (val) {
                      setModalState(() {
                        selectedMethod = val!;
                      });
                    },
                  ),
                  const SizedBox(height: 16),
                  CustomButton(
                    text: 'Proceed to Pay',
                    onPressed: () async {
                      Navigator.pop(ctx);
                      setState(() {
                        _isActionLoading = true;
                      });
                      try {
                        final url = await _apiService.createAppointmentPaymentUrl(
                          appointmentId: widget.appointmentId,
                          paymentMethod: selectedMethod,
                        );
                        if (url != null && url.isNotEmpty) {
                          final uri = Uri.parse(url);
                          if (await canLaunchUrl(uri)) {
                            await launchUrl(uri, mode: LaunchMode.externalApplication);
                          }
                        } else {
                          _showError('Could not generate payment URL.');
                        }
                      } catch (e) {
                        _showError('Payment error: $e');
                      } finally {
                        setState(() {
                          _isActionLoading = false;
                        });
                      }
                    },
                  ),
                ],
              ),
            );
          },
        );
      },
    );
  }

  void _showReviewDialog() {
    int rating = 5;
    final noteCtrl = TextEditingController();

    showDialog(
      context: context,
      builder: (ctx) {
        return StatefulBuilder(
          builder: (context, setDialogState) {
            return AlertDialog(
              title: const Text('Rate & Review Service'),
              content: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  const Text('How was your experience?'),
                  const SizedBox(height: 12),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: List.generate(5, (index) {
                      final starVal = index + 1;
                      return IconButton(
                        icon: Icon(
                          starVal <= rating ? Icons.star : Icons.star_border,
                          color: Colors.amber,
                          size: 32,
                        ),
                        onPressed: () {
                          setDialogState(() {
                            rating = starVal;
                          });
                        },
                      );
                    }),
                  ),
                  const SizedBox(height: 12),
                  TextFormField(
                    controller: noteCtrl,
                    maxLines: 3,
                    decoration: const InputDecoration(
                      hintText: 'Write your feedback here...',
                      border: OutlineInputBorder(),
                    ),
                  ),
                ],
              ),
              actions: [
                TextButton(
                  onPressed: () => Navigator.pop(ctx),
                  child: const Text('Cancel'),
                ),
                ElevatedButton(
                  style: ElevatedButton.styleFrom(backgroundColor: AppColors.primary),
                  onPressed: () async {
                    Navigator.pop(ctx);
                    setState(() {
                      _isActionLoading = true;
                    });
                    try {
                      final ok = await _apiService.submitAppointmentReview(
                        SubmitAppointmentReviewRequest(
                          appointmentId: widget.appointmentId,
                          rating: rating,
                          reviewNote: noteCtrl.text.trim(),
                        ),
                      );
                      if (ok) {
                        ScaffoldMessenger.of(context).showSnackBar(
                          const SnackBar(content: Text('Thank you for your feedback!'), backgroundColor: Colors.green),
                        );
                        _loadDetail();
                      } else {
                        _showError('Failed to submit review.');
                      }
                    } catch (e) {
                      _showError('Review error: $e');
                    } finally {
                      setState(() {
                        _isActionLoading = false;
                      });
                    }
                  },
                  child: const Text('Submit Review', style: TextStyle(color: Colors.white)),
                ),
              ],
            );
          },
        );
      },
    );
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
        title: const Text('Appointment Detail'),
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
      ),
      body: FutureBuilder<AppointmentDetailModel>(
        future: _detailFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting && _detail == null) {
            return const Center(child: CircularProgressIndicator());
          }

          if (snapshot.hasError) {
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Icon(Icons.error_outline, size: 64, color: AppColors.error),
                  const SizedBox(height: 16),
                  Text('Failed to load detail: ${snapshot.error}', textAlign: TextAlign.center),
                  const SizedBox(height: 16),
                  ElevatedButton(onPressed: _loadDetail, child: const Text('Reload')),
                ],
              ),
            );
          }

          final detail = snapshot.data!;
          final statusColor = _getStatusColor(detail.status);
          final dateStartStr = DateFormat('EEE, dd/MM/yyyy HH:mm').format(detail.appointmentStart);
          final dateEndStr = DateFormat('HH:mm').format(detail.appointmentEnd);

          return SingleChildScrollView(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Header Status Card
                Card(
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            const Text('Appointment Status', style: TextStyle(color: AppColors.textSecondary, fontSize: 13)),
                            const SizedBox(height: 4),
                            Text(
                              detail.statusText,
                              style: TextStyle(color: statusColor, fontWeight: FontWeight.bold, fontSize: 18),
                            ),
                          ],
                        ),
                        Container(
                          padding: const EdgeInsets.all(12),
                          decoration: BoxDecoration(
                            color: statusColor.withAlpha(30),
                            shape: BoxShape.circle,
                          ),
                          child: Icon(Icons.event_available, color: statusColor, size: 32),
                        ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 16),

                // Pet & Vet Info Card
                Card(
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      children: [
                        ListTile(
                          contentPadding: EdgeInsets.zero,
                          leading: const CircleAvatar(
                            backgroundColor: AppColors.primary,
                            child: Icon(Icons.pets, color: Colors.white),
                          ),
                          title: Text(detail.petName, style: const TextStyle(fontWeight: FontWeight.bold)),
                          subtitle: const Text('Pet Patient'),
                        ),
                        const Divider(height: 16),
                        ListTile(
                          contentPadding: EdgeInsets.zero,
                          leading: const CircleAvatar(
                            backgroundColor: Colors.teal,
                            child: Icon(Icons.medical_services_outlined, color: Colors.white),
                          ),
                          title: Text(detail.vetName.isNotEmpty ? detail.vetName : 'Assigned Doctor'),
                          subtitle: const Text('Doctor / Veterinarian'),
                        ),
                        const Divider(height: 16),
                        ListTile(
                          contentPadding: EdgeInsets.zero,
                          leading: const CircleAvatar(
                            backgroundColor: Colors.orange,
                            child: Icon(Icons.access_time, color: Colors.white),
                          ),
                          title: Text('$dateStartStr - $dateEndStr'),
                          subtitle: const Text('Appointment Time Slot'),
                        ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 16),

                // Services Breakdown Card
                Card(
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text('Booked Services', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                        const SizedBox(height: 12),
                        ListView.separated(
                          shrinkWrap: true,
                          physics: const NeverScrollableScrollPhysics(),
                          itemCount: detail.appointmentServices.length,
                          separatorBuilder: (_, __) => const Divider(),
                          itemBuilder: (context, index) {
                            final svc = detail.appointmentServices[index];
                            return Row(
                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                              children: [
                                Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Text(svc.serviceName, style: const TextStyle(fontWeight: FontWeight.w600)),
                                    Text('${svc.duration} mins', style: const TextStyle(color: AppColors.textSecondary, fontSize: 12)),
                                  ],
                                ),
                                Text(
                                  '${svc.price.toStringAsFixed(0)}đ',
                                  style: const TextStyle(fontWeight: FontWeight.bold, color: AppColors.primary),
                                ),
                              ],
                            );
                          },
                        ),
                        const Divider(height: 24),
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            const Text('Total Cost:', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                            Text(
                              '${detail.total.toStringAsFixed(0)}đ',
                              style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 20, color: AppColors.primary),
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 16),

                // Note & Feedback Card
                if ((detail.note != null && detail.note!.isNotEmpty) || detail.snapshot != null)
                  Card(
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                    child: Padding(
                      padding: const EdgeInsets.all(16),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          if (detail.note != null && detail.note!.isNotEmpty) ...[
                            const Text('Note for Doctor:', style: TextStyle(fontWeight: FontWeight.bold)),
                            const SizedBox(height: 4),
                            Text(detail.note!, style: const TextStyle(color: AppColors.textSecondary)),
                          ],
                          if (detail.snapshot?.feedback != null) ...[
                            const Divider(height: 16),
                            Row(
                              children: [
                                const Text('Your Rating & Feedback: ', style: TextStyle(fontWeight: FontWeight.bold)),
                                Row(
                                  children: List.generate(5, (i) {
                                    return Icon(
                                      i < (detail.snapshot?.rating ?? 0) ? Icons.star : Icons.star_border,
                                      color: Colors.amber,
                                      size: 18,
                                    );
                                  }),
                                ),
                              ],
                            ),
                            const SizedBox(height: 4),
                            Text(detail.snapshot!.feedback!, style: const TextStyle(color: AppColors.textSecondary)),
                          ],
                        ],
                      ),
                    ),
                  ),
                const SizedBox(height: 24),

                // Action Buttons
                if (_isActionLoading)
                  const Center(child: CircularProgressIndicator())
                else Column(
                  children: [
                    // If Pending (Status == 1): Pay Online, Edit, Cancel
                    if (detail.status == 1) ...[
                      CustomButton(
                        text: 'Pay Online Now (VNPAY / MOMO)',
                        onPressed: _showPaymentModal,
                      ),
                      const SizedBox(height: 12),
                      OutlinedButton(
                        style: OutlinedButton.styleFrom(
                          minimumSize: const Size(double.infinity, 48),
                          side: const BorderSide(color: AppColors.primary),
                        ),
                        onPressed: () async {
                          final updated = await Navigator.push<bool>(
                            context,
                            MaterialPageRoute(
                              builder: (context) => AppointmentUpdateScreen(detail: detail),
                            ),
                          );
                          if (updated == true) _loadDetail();
                        },
                        child: const Text('Edit Appointment'),
                      ),
                      const SizedBox(height: 12),
                      TextButton(
                        onPressed: _cancelAppointment,
                        child: const Text('Cancel Appointment', style: TextStyle(color: Colors.red)),
                      ),
                    ],

                    // If Completed (Status == 3): Write Review
                    if (detail.status == 3 && detail.snapshot?.feedback == null) ...[
                      CustomButton(
                        text: 'Submit Review & Rating',
                        onPressed: _showReviewDialog,
                      ),
                    ],
                  ],
                ),
              ],
            ),
          );
        },
      ),
    );
  }
}
