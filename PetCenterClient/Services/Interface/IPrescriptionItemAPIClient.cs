using static PetCenterClient.ViewModels.PrescriptionItem.PrescriptionItemViewModel;

namespace PetCenterClient.Services.Interface
{
    public interface IPrescriptionItemAPIClient
    {
        Task<IEnumerable<ReadPrescriptionItemViewModel>> GetByRecordAsync(Guid recordId);
        Task<ReadPrescriptionItemViewModel?> GetByIdAsync(Guid id);
        Task CreateAsync(CreatePrescriptionItemViewModel model);
        Task UpdateAsync(Guid id, UpdatePrescriptionItemViewModel model);
        Task DeleteAsync(Guid id);
    }
}
