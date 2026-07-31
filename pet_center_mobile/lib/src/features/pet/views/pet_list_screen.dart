import 'package:flutter/material.dart';
import '../../../constants/app_colors.dart';
import '../../../models/pet_model.dart';
import '../../../services/api_service.dart';
import 'pet_detail_screen.dart';
import 'pet_form_screen.dart';

class PetListScreen extends StatefulWidget {
  const PetListScreen({super.key});

  @override
  State<PetListScreen> createState() => _PetListScreenState();
}

class _PetListScreenState extends State<PetListScreen> {
  final ApiService _apiService = ApiService();
  late Future<List<PetModel>> _petsFuture;

  @override
  void initState() {
    super.initState();
    _loadPets();
  }

  void _loadPets() {
    setState(() {
      _petsFuture = _apiService.getMyPets();
    });
  }

  Widget _buildPetCard(PetModel pet) {
    final avatarUrl = pet.getFullAvatarUrl(ApiService.baseUrl);
    final isMale = pet.gender.toLowerCase() == 'male';

    return Card(
      margin: const EdgeInsets.only(bottom: 14),
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: InkWell(
        borderRadius: BorderRadius.circular(16),
        onTap: () async {
          final updated = await Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => PetDetailScreen(petId: pet.petId),
            ),
          );
          if (updated == true) {
            _loadPets();
          }
        },
        child: Padding(
          padding: const EdgeInsets.all(14.0),
          child: Row(
            children: [
              // Avatar
              ClipRRect(
                borderRadius: BorderRadius.circular(14),
                child: avatarUrl != null
                    ? Image.network(
                        avatarUrl,
                        width: 76,
                        height: 76,
                        fit: BoxFit.cover,
                        errorBuilder: (context, error, stackTrace) =>
                            _buildPlaceholderAvatar(pet),
                      )
                    : _buildPlaceholderAvatar(pet),
              ),
              const SizedBox(width: 14),
              // Info
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Expanded(
                          child: Text(
                            pet.petName,
                            style: const TextStyle(
                              fontSize: 17,
                              fontWeight: FontWeight.bold,
                              color: AppColors.textPrimary,
                            ),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                        ),
                        Container(
                          padding: const EdgeInsets.symmetric(
                              horizontal: 8, vertical: 3),
                          decoration: BoxDecoration(
                            color: isMale
                                ? Colors.blue.withOpacity(0.12)
                                : Colors.pink.withOpacity(0.12),
                            borderRadius: BorderRadius.circular(8),
                          ),
                          child: Text(
                            pet.gender.isNotEmpty ? pet.gender : 'Unknown',
                            style: TextStyle(
                              color: isMale
                                  ? Colors.blue.shade700
                                  : Colors.pink.shade700,
                              fontSize: 11,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 6),
                    Text(
                      '${pet.species} • ${pet.breed}',
                      style: const TextStyle(
                        fontSize: 13,
                        color: AppColors.textSecondary,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Row(
                      children: [
                        if (pet.weight != null && pet.weight! > 0) ...[
                          const Icon(Icons.scale_outlined,
                              size: 14, color: AppColors.primary),
                          const SizedBox(width: 4),
                          Text(
                            '${pet.weight} kg',
                            style: const TextStyle(
                                fontSize: 12, fontWeight: FontWeight.w500),
                          ),
                          const SizedBox(width: 12),
                        ],
                        const Icon(Icons.cake_outlined,
                            size: 14, color: AppColors.primary),
                        const SizedBox(width: 4),
                        Expanded(
                          child: Text(
                            pet.ageDisplay,
                            style: const TextStyle(
                                fontSize: 12, fontWeight: FontWeight.w500),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildPlaceholderAvatar(PetModel pet) {
    IconData icon = Icons.pets;
    final speciesLower = pet.species.toLowerCase();
    if (speciesLower == 'cat') {
      icon = Icons.pets; // Có thể dùng font custom để chia hình ảnh chó/mèo
    }

    return Container(
      width: 76,
      height: 76,
      color: AppColors.primary.withOpacity(0.12),
      child: Icon(icon, size: 38, color: AppColors.primary),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('My Pets'),
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
        elevation: 0,
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: _loadPets,
          ),
        ],
      ),
      body: FutureBuilder<List<PetModel>>(
        future: _petsFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }

          if (snapshot.hasError) {
            return Center(
              child: Padding(
                padding: const EdgeInsets.all(24.0),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    const Icon(Icons.error_outline,
                        size: 60, color: AppColors.error),
                    const SizedBox(height: 12),
                    Text(
                      'Failed to load pets: ${snapshot.error}',
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 16),
                    ElevatedButton(
                      onPressed: _loadPets,
                      child: const Text('Retry'),
                    ),
                  ],
                ),
              ),
            );
          }

          final pets = snapshot.data ?? [];

          if (pets.isEmpty) {
            return Center(
              child: Padding(
                padding: const EdgeInsets.all(24.0),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(Icons.pets_outlined,
                        size: 80, color: Colors.grey.shade400),
                    const SizedBox(height: 16),
                    const Text(
                      'No pets found',
                      style:
                          TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                    ),
                    const SizedBox(height: 8),
                    const Text(
                      'Add your pet profiles to easily book services and shop for products.',
                      textAlign: TextAlign.center,
                      style: TextStyle(color: AppColors.textSecondary),
                    ),
                    const SizedBox(height: 24),
                    ElevatedButton.icon(
                      style: ElevatedButton.styleFrom(
                        backgroundColor: AppColors.primary,
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(
                            horizontal: 24, vertical: 12),
                        shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(10)),
                      ),
                      icon: const Icon(Icons.add),
                      label: const Text('Add New Pet',
                          style: TextStyle(fontWeight: FontWeight.bold)),
                      onPressed: () async {
                        final created = await Navigator.push(
                          context,
                          MaterialPageRoute(
                              builder: (context) => const PetFormScreen()),
                        );
                        if (created == true) _loadPets();
                      },
                    ),
                  ],
                ),
              ),
            );
          }

          return RefreshIndicator(
            onRefresh: () async => _loadPets(),
            child: ListView.builder(
              padding: const EdgeInsets.all(16.0),
              itemCount: pets.length,
              itemBuilder: (context, index) {
                return _buildPetCard(pets[index]);
              },
            ),
          );
        },
      ),
      floatingActionButton: FloatingActionButton.extended(
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
        icon: const Icon(Icons.add),
        label: const Text('Add Pet'),
        onPressed: () async {
          final created = await Navigator.push(
            context,
            MaterialPageRoute(builder: (context) => const PetFormScreen()),
          );
          if (created == true) _loadPets();
        },
      ),
    );
  }
}
