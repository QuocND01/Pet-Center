using PetCenterClient.Common;
using PetCenterClient.ViewModels.Common;
using static PetCenterClient.ViewModels.Service.ServiceViewModel;

namespace PetCenterClient.Services.Interface
{
    public interface IServiceAPIClient
    {
        Task<OdataResponse<ReadServiceViewModelForCustomer>> GetAllServiceAsync(
                string? search,
                decimal? minPrice,
                decimal? maxPrice,
                int? serviceType,
                int page = 1);

        Task<PagedResponse<ReadServiceViewModel>> GetAllServiceAdminAsync(
     string? search,
     bool? isActive,
     decimal? minPrice,
     decimal? maxPrice,
     int? serviceType,
     int page = 1,
     int pageSize = 10);

        Task<ReadServiceViewModel> DetailsServiceAsync(Guid? id);
        Task AddServiceAsync(CreateServiceViewModel createService);
        Task UpdateServiceAsync(Guid? id, UpdateServiceViewModel updateService);
        Task ChangeServiceStatusAsync(
      Guid id,
      Status status);

    }
}
