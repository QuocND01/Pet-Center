using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;
using static PetCenterAPI.DTOs.Requests.CustomerProfile.PetRequestDTO;

namespace PetCenterAPI.Service
{

    public class PetService : IPetService
    {
        private readonly IPetRepository _petRepository;

        public PetService(IPetRepository petRepository)
        {
            _petRepository = petRepository;
        }

        private string CalculateAge(DateOnly? dob)
        {
            if (!dob.HasValue) return "Unknown";
            var age = DateTime.Today.Year - dob.Value.Year;
            return age > 0 ? $"{age} years" : "Under 1 year";
        }

        public async Task<List<ReadPetListDTO>> GetMyPetsAsync(Guid customerId)
        {
            var pets = await _petRepository.GetPetsByCustomerIdAsync(customerId);
            return pets.Select(p => new ReadPetListDTO
            {
                PetId = p.PetId,
                Species = p.Species ?? "Unknown",
                Breed = p.Breed ?? "Unknown",
                Gender = p.Gender ?? "Unknown",
                PetAvatar = p.PetAvatar,
                Age = CalculateAge(p.DateOfBirth),
                IsActive = p.IsActive ?? true
            }).ToList();
        }

        public async Task<ReadPetDetailDTO?> GetPetDetailsAsync(Guid petId, Guid customerId)
        {
            var p = await _petRepository.GetPetByIdAsync(petId, customerId);
            if (p == null) return null;

            return new ReadPetDetailDTO
            {
                PetId = p.PetId,
                Species = p.Species ?? "Unknown",
                Breed = p.Breed ?? "Unknown",
                Gender = p.Gender ?? "Unknown",
                PetAvatar = p.PetAvatar,
                Age = CalculateAge(p.DateOfBirth),
                IsActive = p.IsActive ?? true,
                Weight = p.Weight,
                Note = p.Note,
                DateOfBirth = p.DateOfBirth
            };
        }
    }
}