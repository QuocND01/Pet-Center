using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IBrandService
    {
        Task<IEnumerable<ReadBrandDTOs>> GetAllBrandAsync();
    }
}
