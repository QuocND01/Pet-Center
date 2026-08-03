using PetCenterClient.ViewModels.Inventory;

namespace PetCenterClient.Services.Interface
{
    public interface IInventoryApiService
    {
        Task<InventoryListResponseViewModel?> GetPagedAsync(
    string? keyword,
    Guid? categoryId,
    Guid? brandId,
    bool? lowStock,
    bool? outOfStock,
    int page = 1,
    int pageSize = 10);
        Task<InventoryDetailViewModel?> GetByIdAsync(Guid id);
        Task<HttpResponseMessage> DownloadExcelReportAsync();
    }
}
