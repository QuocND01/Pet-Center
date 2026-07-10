using PetCenterAPI.DTOs.Requests;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;
using static PetCenterAPI.DTOs.Requests.CustomerProfile.PetRequestDTO;

namespace PetCenterAPI.Service
{
    public class PetService : IPetService
    {
        private readonly IPetRepository _petRepository;
        private readonly PetCenterContext _db;
        private readonly ICloudinaryService _cloudinaryService;


        public PetService(IPetRepository petRepository, PetCenterContext db, ICloudinaryService cloudinaryService)
        {
            _petRepository = petRepository;
            _db = db;
            _cloudinaryService = cloudinaryService;
        }

        public IQueryable<ReadPetListDTO> GetMyPetsQuery(Guid customerId)
        {
            return _db.Pets
                .Where(p => p.CustomerId == customerId && (p.IsActive ?? true) == true)
                .Select(p => new ReadPetListDTO
                {
                    PetId = p.PetId,
                    PetName = p.PetName ?? "Unknown",
                    Species = p.Species ?? "Unknown",
                    Breed = p.Breed ?? "Unknown",
                    Gender = p.Gender ?? "Unknown",
                    PetAvatar = p.PetAvatar,
                    DateOfBirth = p.DateOfBirth,
                    IsActive = p.IsActive ?? true
                });
        }

        public async Task<ReadPetDetailDTO?> GetPetDetailsAsync(Guid petId, Guid customerId)
        {
            var p = await _petRepository.GetPetByIdAsync(petId, customerId);
            if (p == null) return null;

            return new ReadPetDetailDTO
            {
                PetId = p.PetId,
                PetName = p.PetName ?? "Unknown",
                Species = p.Species ?? "Unknown",
                Breed = p.Breed ?? "Unknown",
                Gender = p.Gender ?? "Unknown",
                PetAvatar = p.PetAvatar,
                DateOfBirth = p.DateOfBirth,
                IsActive = p.IsActive ?? true,
                Weight = p.Weight,
                Note = p.Note
            };
        }

        public IQueryable<VetPetRequestDTO.ReadVetPetListDTO> GetAllPetsForVetQuery()
        {
            return _db.Pets
                .Where(p => (p.IsActive ?? true) == true)
                .Select(p => new VetPetRequestDTO.ReadVetPetListDTO
                {
                    PetId = p.PetId,
                    PetName = p.PetName ?? "Unknown",
                    Species = p.Species ?? "Unknown",
                    Breed = p.Breed ?? "Unknown",
                    Gender = p.Gender ?? "Unknown",
                    PetAvatar = p.PetAvatar,
                    DateOfBirth = p.DateOfBirth,
                    OwnerName = p.Customer.FullName ?? "Unknown",
                    OwnerPhone = p.Customer.PhoneNumber ?? "Unknown"
                });
        }

        public async Task<VetPetRequestDTO.ReadVetPetDetailDTO?> GetPetDetailForVetAsync(Guid petId)
        {
            var p = await _petRepository.GetPetByIdWithOwnerAsync(petId);
            if (p == null) return null;

            return new VetPetRequestDTO.ReadVetPetDetailDTO
            {
                PetId = p.PetId,
                PetName = p.PetName ?? "Unknown",
                Species = p.Species ?? "Unknown",
                Breed = p.Breed ?? "Unknown",
                Gender = p.Gender ?? "Unknown",
                PetAvatar = p.PetAvatar,
                OwnerName = p.Customer?.FullName ?? "Unknown",
                OwnerPhone = p.Customer?.PhoneNumber ?? "Unknown",
                Weight = p.Weight,
                Note = p.Note,
                DateOfBirth = p.DateOfBirth
            };
        }

        public async Task<bool> AddPetAsync(Guid customerId, MutatePetDTO dto)
        {
            var pet = new Pet
            {
                PetId = Guid.NewGuid(),
                CustomerId = customerId,
                PetName = dto.PetName,
                Species = dto.Species,
                Breed = dto.Breed,
                Gender = dto.Gender,
                Weight = dto.Weight,
                Note = dto.Note,
                DateOfBirth = dto.DateOfBirth,
                IsActive = true
            };

            // LOGIC UPLOAD ẢNH CLOUDINARY
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var uploadResult = await _cloudinaryService.UploadImageAsync(dto.ImageFile, "pets");
                if (uploadResult != null && uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    pet.PetAvatar = uploadResult.SecureUrl.ToString();
                }
            }

            await _petRepository.AddPetAsync(pet);
            await _petRepository.SaveAsync();
            return true;
        }

        public async Task<bool> UpdatePetAsync(Guid petId, Guid customerId, MutatePetDTO dto, bool isVet)
        {
            var pet = isVet ? await _petRepository.GetPetByIdWithOwnerAsync(petId)
                            : await _petRepository.GetPetByIdAsync(petId, customerId);

            if (pet == null) return false;

            pet.Species = dto.Species;
            pet.PetName = dto.PetName;
            pet.Breed = dto.Breed;
            pet.Gender = dto.Gender;
            pet.Weight = dto.Weight;
            pet.Note = dto.Note;
            pet.DateOfBirth = dto.DateOfBirth;

            // LOGIC UPLOAD ẢNH MỚI CLOUDINARY
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var uploadResult = await _cloudinaryService.UploadImageAsync(dto.ImageFile, "pets");
                if (uploadResult != null && uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    pet.PetAvatar = uploadResult.SecureUrl.ToString();
                }
            }

            await _petRepository.UpdatePetAsync(pet);
            await _petRepository.SaveAsync();
            return true;
        }

        public async Task<bool> DeletePetAsync(Guid petId, Guid customerId, bool isVet)
        {
            var pet = isVet ? await _petRepository.GetPetByIdWithOwnerAsync(petId)
                            : await _petRepository.GetPetByIdAsync(petId, customerId);

            if (pet == null) return false;

            pet.IsActive = false; // Xóa mềm
            await _petRepository.UpdatePetAsync(pet);
            await _petRepository.SaveAsync();
            return true;
        }
    }
}