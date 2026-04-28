using AttendanceAPI.DTOs;

namespace AttendanceAPI.Service.Interface
{
    public interface IStaffShiftService
    {
        Task<IEnumerable<StaffShiftResponseDTO>> GetShiftsAsync(StaffShiftQueryParameters query);
        Task<StaffShiftResponseDTO?> GetShiftDetailsAsync(Guid id);
        Task<StaffShiftResponseDTO> CreateShiftAsync(StaffShiftRequestDTO dto);
        Task<bool> ChangeShiftStatusAsync(Guid shiftId, int newStatus);
    }
}
