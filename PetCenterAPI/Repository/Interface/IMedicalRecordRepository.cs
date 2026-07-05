using PetCenterAPI.Common;
using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface IMedicalRecordRepository
    {
        Task<(IEnumerable<MedicalRecord> Items, int Total)> GetAllAsync(string? search, int? status, int page, int pageSize);
        Task<IEnumerable<MedicalRecord>> GetByCustomerIdAsync(Guid customerId, string? search);
        Task<MedicalRecord?> GetByIdAsync(Guid id);
        Task<IEnumerable<Appointment>> GetCompletedAppointmentsAsync();
        Task AddAsync(MedicalRecord record);
        Task UpdateAsync(MedicalRecord record);
        Task ChangeStatusAsync(Guid id, MedicalRecordStatus status);
        Task<Disease?> GetDiseaseByIdAsync(Guid id);
        Task<IEnumerable<Disease>> GetActiveDiseasesAsync(int? species);
    }
}
