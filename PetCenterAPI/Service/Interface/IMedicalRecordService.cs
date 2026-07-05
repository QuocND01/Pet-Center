using PetCenterAPI.Common;
using static PetCenterAPI.DTOs.Requests.MedicalRecord.MedicalRecordRequestDTO;
using static PetCenterAPI.DTOs.Responses.MedicalRecord.MedicalRecordResponseDTO;

namespace PetCenterAPI.Service.Interface
{
    public interface IMedicalRecordService
    {
        Task<(IEnumerable<ReadMedicalRecordListDTO> Items, int Total)> GetAllAsync(string? search, int? status, int page, int pageSize);
        Task<IEnumerable<ReadMedicalRecordListDTO>> GetByCustomerIdAsync(Guid customerId, string? search);
        Task<ReadMedicalRecordDTO?> GetByIdAsync(Guid id);
        Task<IEnumerable<CompletedAppointmentDTO>> GetCompletedAppointmentsAsync();
        Task CreateAsync(CreateMedicalRecordDTO dto);
        Task UpdateAsync(Guid id, UpdateMedicalRecordDTO dto);
        Task ChangeStatusAsync(Guid id, MedicalRecordStatus status);
        Task<IEnumerable<ReadDiseaseDTO>> GetActiveDiseasesAsync(int? species);
    }
}
