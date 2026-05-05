using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IBrandAPIClient
    {
        Task<OdataResponse<ReadBrandDTOs>> GetAllBrandAsync(string? search, int page = 1);

        Task<ReadBrandDTOs> GetBrandByIdAsync(Guid? id);
        Task AddBrandAsync(CreateBrandDTOs createBrand);
        Task UpdateBrandAsync(Guid? id, UpdateBrandDTOs updateBrand);
        Task DeleteBrandAsync(Guid? id);
    }
}
