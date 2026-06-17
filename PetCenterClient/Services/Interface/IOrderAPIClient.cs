using PetCenterClient.Common;
using PetCenterClient.DTOs;
using PetCenterClient.ViewModels.Common;

namespace PetCenterClient.Services.Interface
{
    public interface IOrderAPIClient
    {
        Task<OdataResponse<ReadOrderListViewModel>> GetOrderListAdminAsync(
            string? search,
            int? status,
            string? paymentMethod,
            string? sortBy,
            string sortOrder = "desc",
            int page = 1);

        Task<ReadOrderDetailViewModel> GetOrderDetailsAsync(Guid id);

        Task<bool> CancelOrderAsync(Guid id);
        Task<bool> AdvanceOrderStatusAsync(Guid id);
    }
}