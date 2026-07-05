using PetCenterClient.ViewModels.Common;
using static PetCenterClient.ViewModels.MedicalRecord.MedicalRecordViewModel;
using static PetCenterClient.ViewModels.PrescriptionItem.PrescriptionItemViewModel;

namespace PetCenterClient.Services.Interface
{
    public interface IMedicalRecordAPIClient
    {
        Task<PagedResponse<ReadMedicalRecordListViewModel>> GetAllAdminAsync(string? search, int? status, int page, int pageSize = 10);
        Task<IEnumerable<ReadMedicalRecordListViewModel>> GetByCustomerAsync(Guid customerId, string? search);
        Task<ReadMedicalRecordDetailViewModel?> GetByIdAsync(Guid id);
        Task<IEnumerable<CompletedAppointmentViewModel>> GetCompletedAppointmentsAsync();
        Task CreateAsync(CreateMedicalRecordViewModel model);
        Task UpdateAsync(Guid id, UpdateMedicalRecordViewModel model);
        Task ChangeStatusAsync(Guid id, int status);
        Task<IEnumerable<ReadDiseaseViewModel>> GetDiseasesAsync(int? species);
    }
}
