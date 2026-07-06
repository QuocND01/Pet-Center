using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface IPetRepository
    {
        Task<List<Pet>> GetPetsByCustomerIdAsync(Guid customerId);
        Task<Pet?> GetPetByIdAsync(Guid petId, Guid customerId);

        Task<List<Pet>> GetAllPetsWithOwnersAsync();
        Task<Pet?> GetPetByIdWithOwnerAsync(Guid petId);
    }
}