using PayrollAPI.DTOs;

namespace PayrollAPI.Service.Interface
{
    public interface IViolationService
    {
        Task<IEnumerable<ViolationResponseDTO>> GetViolationsAsync(ViolationQueryParameters query);
        Task<ViolationResponseDTO?> GetViolationDetailsAsync(Guid id);
        Task<ViolationResponseDTO> CreateViolationAsync(ViolationRequestDTO dto);
        Task<bool> ChangeViolationStatusAsync(Guid id, int newStatus);
    }
}
