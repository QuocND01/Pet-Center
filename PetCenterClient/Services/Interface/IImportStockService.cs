using PetCenterClient.ViewModels;

namespace PetCenterClient.Services.Interface
{
    public interface IImportStockService
    {
        Task<List<ImportHeaderViewModel>> GetAllAsync();
        Task<ImportStockViewModel?> GetByIdAsync(Guid id);
        Task<Guid> CreateAsync(CreateImportViewModel dto);
        Task ConfirmAsync(Guid id);
        Task CancelAsync(Guid id);

        
    }
}
