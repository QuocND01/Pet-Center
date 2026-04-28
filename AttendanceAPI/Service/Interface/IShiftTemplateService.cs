using AttendanceAPI.DTOs;

namespace AttendanceAPI.Service.Interface
{
    public interface IShiftTemplateService
    {
        Task<IEnumerable<ShiftTemplateResponseDTO>> GetTemplatesAsync(ShiftTemplateQueryParameters query);
        Task<ShiftTemplateResponseDTO?> GetTemplateDetailsAsync(Guid id);
        Task<ShiftTemplateResponseDTO> CreateTemplateAsync(ShiftTemplateRequestDTO dto);
        Task<bool> UpdateTemplateAsync(Guid id, ShiftTemplateRequestDTO dto);
        Task<bool> DeleteTemplateAsync(Guid id);// Soft Delete
    }
}
