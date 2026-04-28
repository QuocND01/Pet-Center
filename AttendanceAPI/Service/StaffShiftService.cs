using AttendanceAPI.DTOs;
using AttendanceAPI.Models;
using AttendanceAPI.Repository.Interface;
using AttendanceAPI.Service.Interface;
using AutoMapper;

namespace AttendanceAPI.Service
{
    public class StaffShiftService : IStaffShiftService
    {
        private readonly IStaffShiftRepository _repo;
        private readonly IMapper _mapper;

        public StaffShiftService(IStaffShiftRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        // View All / Filter / Search
        public async Task<IEnumerable<StaffShiftResponseDTO>> GetShiftsAsync(StaffShiftQueryParameters query)
        {
            var shifts = await _repo.GetAllWithFilterAsync(query);
            return _mapper.Map<IEnumerable<StaffShiftResponseDTO>>(shifts);
        }

        // View Details
        public async Task<StaffShiftResponseDTO?> GetShiftDetailsAsync(Guid id)
        {
            var shift = await _repo.GetByIdAsync(id);
            return _mapper.Map<StaffShiftResponseDTO>(shift);
        }

        // Add
        public async Task<StaffShiftResponseDTO> CreateShiftAsync(StaffShiftRequestDTO dto)
        {
            var shift = _mapper.Map<StaffShift>(dto);
            shift.ShiftId = Guid.NewGuid();
            shift.Status = 1; // Default SQL đang là 1

            await _repo.AddAsync(shift);
            await _repo.SaveChangesAsync();

            // Trả về DTO (gọi GetById để có Include Template map DTO trả về cho đẹp)
            var createdShift = await _repo.GetByIdAsync(shift.ShiftId);
            return _mapper.Map<StaffShiftResponseDTO>(createdShift);
        }

        // Change Status
        public async Task<bool> ChangeShiftStatusAsync(Guid shiftId, int newStatus)
        {
            var shift = await _repo.GetByIdAsync(shiftId);
            if (shift == null) return false;

            shift.Status = newStatus;
            _repo.Update(shift);

            return await _repo.SaveChangesAsync();
        }
    }
}