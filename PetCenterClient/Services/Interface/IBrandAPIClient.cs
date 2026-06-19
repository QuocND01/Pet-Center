using PetCenterClient.Common;
using PetCenterClient.ViewModels.Brand;
using PetCenterClient.ViewModels.Common;

namespace PetCenterClient.Services.Interface
{
    public interface IBrandAPIClient
    {
        Task<OdataResponse<ReadBrandViewModelForCustomer>> GetAllBrandAsync();
        Task<PagedResponse<ReadBrandViewModel>> GetAllBrandAdminAsync(
       string? search, Status? status, int page = 1, int pageSize = 10);
        Task<ReadBrandViewModel> DetailsBrandAsync(Guid? id);
        Task AddBrandAsync(CreateBrandViewModel createBrand);
        Task UpdateBrandAsync(Guid? id, UpdateBrandViewModel updateBrand);
        Task ChangeBrandStatusAsync(Guid id, Status status);
    }
}
