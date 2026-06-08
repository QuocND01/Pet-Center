using PetCenterClient.Common;
using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IBrandServiceClient
    {
        Task<OdataResponse<ReadBrandDTOForCustomer>> GetAllBrandAsync(string? search, int page = 1);
        Task<PagedResponse<ReadBrandDTO>> GetAllBrandAdminAsync(
       string? search, bool? isActive, int page = 1, int pageSize = 10);
        Task<ReadBrandDTO> DetailsBrandAsync(Guid? id);
        Task AddBrandAsync(CreateBrandDTO createBrand);
        Task UpdateBrandAsync(Guid? id, UpdateBrandDTO updateBrand);
        Task ChangeBrandStatusAsync(Guid id, Status status);
    }
}
