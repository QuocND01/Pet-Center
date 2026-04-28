using AttendanceAPI.DTOs;

namespace AttendanceAPI.Service.Interface
{
    public interface IAttendanceService
    {
        Task<IEnumerable<AttendanceResponseDTO>> GetAttendancesAsync(AttendanceQueryParameters query);
        Task<AttendanceResponseDTO> CreateAttendanceAsync(AttendanceRequestDTO dto);
        Task<bool> UpdateAttendanceStatusAsync(Guid id, int newStatus);
    }
}
