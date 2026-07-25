using PetCenterClient.ViewModels;
using PetCenterClient.ViewModels.Supplier;

namespace PetCenterClient.Services.Interface
{
    public interface ISupplierApiService
    {
        Task<List<ViewSupplierViewModel>> GetAllAsync();

        Task<ViewSupplierViewModel?> GetByIdAsync(Guid id);

        Task<ApiResponseViewModel<ViewSupplierViewModel>?> CreateAsync(CreateSupplierViewModel model);

        Task<ApiResponseViewModel<bool>?> UpdateAsync(
            Guid id,
            CreateSupplierViewModel model);

        Task<bool> DeleteAsync(Guid id);

        Task? GetSupplierSelectAsync();
    }
}