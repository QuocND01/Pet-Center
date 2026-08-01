import 'package:flutter/material.dart';
import '../../../constants/app_colors.dart';
import '../../../models/customer_model.dart';
import '../../../services/api_service.dart';
import '../../address/views/address_list_screen.dart';
import '../../pet/views/pet_list_screen.dart';
import '../../medical_records/views/medical_record_list_screen.dart';
import 'edit_profile_screen.dart';
import '../../auth/views/change_password_screen.dart';

class CustomerProfileScreen extends StatefulWidget {
  const CustomerProfileScreen({super.key});

  @override
  State<CustomerProfileScreen> createState() => _CustomerProfileScreenState();
}

class _CustomerProfileScreenState extends State<CustomerProfileScreen> {
  final ApiService _apiService = ApiService();
  late Future<CustomerModel> _profileFuture;

  @override
  void initState() {
    super.initState();
    _loadProfile();
  }

  void _loadProfile() {
    setState(() {
      _profileFuture = _apiService.getCustomerProfile();
    });
  }

  @override
  Widget build(BuildContext context) {
    if (_apiService.token == null) {
      return Scaffold(
        appBar: AppBar(
          title: const Text('Customer Profile'),
          backgroundColor: AppColors.primary,
          foregroundColor: Colors.white,
        ),
        body: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Icon(Icons.person_outline_rounded, size: 80, color: Colors.grey),
              const SizedBox(height: 16),
              const Text('Please login to view your account details.', style: TextStyle(fontSize: 15, color: AppColors.textSecondary)),
              const SizedBox(height: 16),
              ElevatedButton(
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.primary,
                  padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                ),
                onPressed: () => Navigator.pushNamed(context, '/login'),
                child: const Text('Login Now', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
              ),
            ],
          ),
        ),
      );
    }

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('My Profile', style: TextStyle(fontWeight: FontWeight.bold)),
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
        elevation: 0,
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            tooltip: 'Refresh Profile',
            onPressed: _loadProfile,
          ),
        ],
      ),
      body: FutureBuilder<CustomerModel>(
        future: _profileFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator(color: AppColors.primary));
          }

          if (snapshot.hasError) {
            return _buildMockProfile();
          }

          if (!snapshot.hasData) {
            return const Center(child: Text('Profile information not found.'));
          }

          final customer = snapshot.data!;
          return _buildProfileContent(customer);
        },
      ),
    );
  }

  String _getInitials(String? name) {
    if (name == null || name.trim().isEmpty) return '?';
    final parts = name.trim().split(RegExp(r'\s+'));
    if (parts.length == 1) {
      return parts[0][0].toUpperCase();
    }
    return '${parts[0][0]}${parts[parts.length - 1][0]}'.toUpperCase();
  }

  Widget _buildProfileContent(CustomerModel customer) {
    final initials = _getInitials(customer.fullName);

    return SingleChildScrollView(
      child: Column(
        children: [
          // Hero Header Banner
          Container(
            width: double.infinity,
            decoration: const BoxDecoration(
              color: AppColors.primary,
              borderRadius: BorderRadius.only(
                bottomLeft: Radius.circular(28),
                bottomRight: Radius.circular(28),
              ),
            ),
            padding: const EdgeInsets.fromLTRB(16, 8, 16, 24),
            child: Column(
              children: [
                // Avatar with Edit Badge Button
                Stack(
                  children: [
                    Container(
                      padding: const EdgeInsets.all(4),
                      decoration: BoxDecoration(
                        shape: BoxShape.circle,
                        color: Colors.white.withAlpha(60),
                      ),
                      child: CircleAvatar(
                        radius: 44,
                        backgroundColor: Colors.white,
                        child: Text(
                          initials,
                          style: const TextStyle(
                            fontSize: 28,
                            fontWeight: FontWeight.bold,
                            color: AppColors.primary,
                          ),
                        ),
                      ),
                    ),
                    Positioned(
                      bottom: 0,
                      right: 0,
                      child: GestureDetector(
                        onTap: () async {
                          final result = await Navigator.push(
                            context,
                            MaterialPageRoute(
                              builder: (context) => EditCustomerProfileScreen(customer: customer),
                            ),
                          );
                          if (result == true) {
                            _loadProfile();
                          }
                        },
                        child: Container(
                          padding: const EdgeInsets.all(6),
                          decoration: BoxDecoration(
                            color: Colors.amber.shade700,
                            shape: BoxShape.circle,
                            border: Border.all(color: Colors.white, width: 2),
                            boxShadow: [
                              BoxShadow(
                                color: Colors.black.withAlpha(30),
                                blurRadius: 4,
                              ),
                            ],
                          ),
                          child: const Icon(Icons.edit, color: Colors.white, size: 14),
                        ),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 12),

                // Name & Email
                Text(
                  customer.fullName ?? 'Name not set',
                  textAlign: TextAlign.center,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    fontSize: 20,
                    fontWeight: FontWeight.bold,
                    color: Colors.white,
                  ),
                ),
                if (customer.email != null && customer.email!.isNotEmpty) ...[
                  const SizedBox(height: 2),
                  Text(
                    customer.email!,
                    textAlign: TextAlign.center,
                    style: TextStyle(fontSize: 13, color: Colors.white.withAlpha(200)),
                  ),
                ],
                const SizedBox(height: 10),

                // Badges Row
                Wrap(
                  alignment: WrapAlignment.center,
                  spacing: 8,
                  runSpacing: 6,
                  children: [
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                      decoration: BoxDecoration(
                        color: (customer.isActive ?? true) ? Colors.green.shade700 : Colors.red.shade700,
                        borderRadius: BorderRadius.circular(100),
                      ),
                      child: Text(
                        (customer.isActive ?? true) ? 'Active' : 'Inactive',
                        style: const TextStyle(color: Colors.white, fontSize: 11, fontWeight: FontWeight.bold),
                      ),
                    ),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                      decoration: BoxDecoration(
                        color: (customer.isVerified ?? false) ? Colors.blue.shade700 : Colors.grey.shade700,
                        borderRadius: BorderRadius.circular(100),
                      ),
                      child: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(
                            (customer.isVerified ?? false) ? Icons.verified : Icons.hourglass_empty,
                            color: Colors.white,
                            size: 12,
                          ),
                          const SizedBox(width: 4),
                          Text(
                            (customer.isVerified ?? false) ? 'Verified' : 'Unverified',
                            style: const TextStyle(color: Colors.white, fontSize: 11, fontWeight: FontWeight.bold),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
          const SizedBox(height: 16),

          // Account Details Card
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16.0),
            child: _buildGroupedCard(
              title: 'ACCOUNT DETAILS',
              children: [
                Padding(
                  padding: const EdgeInsets.all(12.0),
                  child: Column(
                    children: [
                      _buildInfoRow(Icons.email_outlined, 'Email Address', customer.email ?? 'Not updated'),
                      _buildInfoRow(Icons.phone_outlined, 'Phone Number', customer.phoneNumber ?? 'Not updated'),
                      _buildInfoRow(Icons.cake_outlined, 'Date of Birth', customer.birthDay ?? 'Not updated'),
                      _buildInfoRow(Icons.wc_outlined, 'Gender', customer.gender ?? 'Not updated'),
                    ],
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: 14),

          // Grouped Card 1: PET CARE & MEDICAL
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16.0),
            child: _buildGroupedCard(
              title: 'PET CARE & MEDICAL',
              children: [
                _buildMenuTile(
                  icon: Icons.pets_rounded,
                  iconColor: Colors.pink,
                  iconBgColor: Colors.pink.shade50,
                  title: 'My Pets',
                  subtitle: 'View & manage your registered pets',
                  onTap: () {
                    Navigator.push(
                      context,
                      MaterialPageRoute(builder: (context) => const PetListScreen()),
                    );
                  },
                ),
                const Divider(height: 1, indent: 56),
                _buildMenuTile(
                  icon: Icons.medical_information_rounded,
                  iconColor: Colors.teal,
                  iconBgColor: Colors.teal.shade50,
                  title: 'Medical History & Prescriptions',
                  subtitle: 'Examination records & prescriptions',
                  onTap: () {
                    Navigator.push(
                      context,
                      MaterialPageRoute(builder: (context) => const MedicalRecordListScreen()),
                    );
                  },
                ),
              ],
            ),
          ),
          const SizedBox(height: 14),

          // Grouped Card 2: ACCOUNT & SECURITY
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16.0),
            child: _buildGroupedCard(
              title: 'ACCOUNT & SECURITY',
              children: [
                _buildMenuTile(
                  icon: Icons.location_on_rounded,
                  iconColor: AppColors.primary,
                  iconBgColor: AppColors.primary.withAlpha(25),
                  title: 'My Address Book',
                  subtitle: 'Manage shipping & delivery addresses',
                  onTap: () {
                    Navigator.push(
                      context,
                      MaterialPageRoute(builder: (context) => const AddressListScreen()),
                    );
                  },
                ),
                const Divider(height: 1, indent: 56),
                _buildMenuTile(
                  icon: Icons.lock_rounded,
                  iconColor: Colors.amber.shade800,
                  iconBgColor: Colors.amber.shade50,
                  title: 'Change Password',
                  subtitle: 'Update account password & security',
                  onTap: () {
                    Navigator.push(
                      context,
                      MaterialPageRoute(builder: (context) => const ChangePasswordScreen()),
                    );
                  },
                ),
                const Divider(height: 1, indent: 56),
                _buildMenuTile(
                  icon: Icons.edit_note_rounded,
                  iconColor: Colors.indigo,
                  iconBgColor: Colors.indigo.shade50,
                  title: 'Edit Profile Information',
                  subtitle: 'Update personal & contact details',
                  onTap: () async {
                    final result = await Navigator.push(
                      context,
                      MaterialPageRoute(
                        builder: (context) => EditCustomerProfileScreen(customer: customer),
                      ),
                    );
                    if (result == true) {
                      _loadProfile();
                    }
                  },
                ),
              ],
            ),
          ),
          const SizedBox(height: 20),

          // Sign Out Button
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16.0),
            child: SizedBox(
              width: double.infinity,
              height: 48,
              child: TextButton.icon(
                style: TextButton.styleFrom(
                  foregroundColor: AppColors.error,
                  backgroundColor: Colors.red.shade50,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                    side: BorderSide(color: Colors.red.shade200),
                  ),
                ),
                icon: const Icon(Icons.logout_rounded, size: 20),
                label: const Text(
                  'Sign Out',
                  style: TextStyle(fontSize: 15, fontWeight: FontWeight.bold),
                ),
                onPressed: _handleLogout,
              ),
            ),
          ),
          const SizedBox(height: 32),
        ],
      ),
    );
  }

  Widget _buildGroupedCard({required String title, required List<Widget> children}) {
    return Container(
      width: double.infinity,
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: AppColors.inputBorder),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withAlpha(6),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 14, 16, 8),
            child: Text(
              title,
              style: const TextStyle(
                fontSize: 11,
                fontWeight: FontWeight.bold,
                color: AppColors.textSecondary,
                letterSpacing: 0.5,
              ),
            ),
          ),
          ...children,
        ],
      ),
    );
  }

  Widget _buildMenuTile({
    required IconData icon,
    required Color iconColor,
    required Color iconBgColor,
    required String title,
    required String subtitle,
    required VoidCallback onTap,
  }) {
    return ListTile(
      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
      leading: Container(
        padding: const EdgeInsets.all(8),
        decoration: BoxDecoration(
          color: iconBgColor,
          borderRadius: BorderRadius.circular(10),
        ),
        child: Icon(icon, color: iconColor, size: 20),
      ),
      title: Text(
        title,
        style: const TextStyle(fontSize: 14, fontWeight: FontWeight.bold, color: AppColors.textPrimary),
      ),
      subtitle: Text(
        subtitle,
        style: const TextStyle(fontSize: 12, color: AppColors.textSecondary),
      ),
      trailing: const Icon(Icons.chevron_right_rounded, color: Colors.grey, size: 22),
      onTap: onTap,
    );
  }

  Widget _buildInfoRow(IconData icon, String label, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 6.0),
      child: Row(
        children: [
          Icon(icon, color: AppColors.primary, size: 18),
          const SizedBox(width: 12),
          SizedBox(
            width: 100,
            child: Text(
              label,
              style: const TextStyle(fontSize: 12, color: AppColors.textSecondary),
            ),
          ),
          Expanded(
            child: Text(
              value,
              textAlign: TextAlign.end,
              style: const TextStyle(fontSize: 13, fontWeight: FontWeight.bold, color: AppColors.textPrimary),
            ),
          ),
        ],
      ),
    );
  }

  Future<void> _handleLogout() async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: const Row(
          children: [
            Icon(Icons.logout, color: AppColors.error),
            SizedBox(width: 8),
            Text('Sign Out', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
          ],
        ),
        content: const Text('Are you sure you want to sign out of your account?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('Cancel', style: TextStyle(color: Colors.grey)),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(
              backgroundColor: AppColors.error,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
            ),
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text('Sign Out', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
          ),
        ],
      ),
    );

    if (confirm == true) {
      await _apiService.logout();
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Logged out successfully.'),
          backgroundColor: AppColors.primary,
        ),
      );
      Navigator.pushNamedAndRemoveUntil(context, '/login', (route) => false);
    }
  }

  Widget _buildMockProfile() {
    final mockCustomer = CustomerModel(
      customerId: '00000000-0000-0000-0000-000000000000',
      fullName: 'John Doe (Demo)',
      email: 'customer@petcenter.com',
      phoneNumber: '0987654321',
      birthDay: '2000-01-01',
      gender: 'Male',
      isVerified: true,
      isActive: true,
      createdAt: DateTime.now(),
    );

    return Column(
      children: [
        Container(
          width: double.infinity,
          color: Colors.amber.shade100,
          padding: const EdgeInsets.all(8),
          child: const Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(Icons.wifi_off, color: Colors.orange),
              SizedBox(width: 8),
              Text(
                'API connection unavailable. Displaying offline demo profile.',
                style: TextStyle(color: Colors.deepOrange, fontWeight: FontWeight.bold, fontSize: 12),
              ),
            ],
          ),
        ),
        Expanded(child: _buildProfileContent(mockCustomer)),
      ],
    );
  }
}
