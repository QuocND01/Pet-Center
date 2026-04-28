using AttendanceAPI.DTOs;
using AttendanceAPI.Models;
using AttendanceAPI.Repository.Interface;
using AttendanceAPI.Service.Interface;
using AutoMapper;

namespace AttendanceAPI.Service
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _repo;
        private readonly IMapper _mapper;

        public AttendanceService(IAttendanceRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        // Dùng chung cho: Admin xem danh sách & Staff xem lịch sử
        public async Task<IEnumerable<AttendanceResponseDTO>> GetAttendancesAsync(AttendanceQueryParameters query)
        {
            var list = await _repo.GetAllWithFilterAsync(query);
            return _mapper.Map<IEnumerable<AttendanceResponseDTO>>(list);
        }

        // Admin thêm mới thủ công
        public async Task<AttendanceResponseDTO> CreateAttendanceAsync(AttendanceRequestDTO dto)
        {
            var attendance = _mapper.Map<Attendance>(dto);
            attendance.AttendanceId = Guid.NewGuid();
            attendance.CreatedAt = DateTime.Now;

            // Nếu không truyền status lên, mặc định là 1
            if (!attendance.Status.HasValue) attendance.Status = 1;

            await _repo.AddAsync(attendance);
            await _repo.SaveChangesAsync();

            return _mapper.Map<AttendanceResponseDTO>(attendance);
        }

        // Cập nhật trạng thái
        public async Task<bool> UpdateAttendanceStatusAsync(Guid id, int newStatus)
        {
            var attendance = await _repo.GetByIdAsync(id);
            if (attendance == null) return false;

            attendance.Status = newStatus;
            _repo.Update(attendance);
            return await _repo.SaveChangesAsync();
        }
    }
}