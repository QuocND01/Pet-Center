using IdentityAPI.DTOs.Response;
using IdentityAPI.DTOs.Resquest;
using IdentityAPI.Models;
using IdentityAPI.Repository.Interface;
using IdentityAPI.Security;
using IdentityAPI.Service.Interface;

namespace IdentityAPI.Service
{
    public class StaffService : IStaffService
    {
        private readonly IStaffRepository _repo;
        private readonly PasswordService _pwService;
        private readonly ILogger<StaffService> _logger;

        public StaffService(IStaffRepository repo, PasswordService pwService, ILogger<StaffService> logger)
        {
            _repo = repo;
            _pwService = pwService;
            _logger = logger;
        }

        public async Task<IEnumerable<StaffResponseDto>> GetListAsync()
        {
            var staffs = await _repo.GetAllAsync();
            return staffs.Select(MapToDto);
        }

        public async Task<StaffResponseDto?> GetDetailsAsync(Guid id)
        {
            var staff = await _repo.GetByIdAsync(id);
            return staff == null ? null : MapToDto(staff);
        }

        public async Task<IEnumerable<StaffResponseDto>> SearchStaffAsync(string keyword)
        {
            var staffs = await _repo.SearchAsync(keyword);
            return staffs.Select(MapToDto);
        }

        public async Task<bool> CreateStaffAsync(StaffCreateDto dto)
        {
            // 1. Kiểm tra email trùng
            if (await _repo.EmailExistsAsync(dto.Email))
            {
                _logger.LogWarning("CreateStaff: Email {Email} already exists.", dto.Email);
                return false;
            }

            // 2. Lấy role "Staff" từ DB
            var staffRole = await _repo.GetRoleByNameAsync("Staff");
            if (staffRole == null)
            {
                _logger.LogError("CreateStaff: Role 'Staff' not found in database. Please seed the Roles table.");
                return false;
            }

            // 3. Tạo staff mới
            var staff = new Staff
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                BirthDay = dto.BirthDay,
                Gender = dto.Gender,
                HiredDate = DateTime.Now,
                PasswordHash = _pwService.Hash(dto.Password),
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            await _repo.AddAsync(staff);

            // 4. Gán role "Staff" vào bảng StaffRoles
            await _repo.AssignRoleAsync(staff.StaffId, staffRole.RoleId);

            _logger.LogInformation("CreateStaff: Staff {FullName} ({Email}) created and assigned role 'Staff'.", staff.FullName, staff.Email);

            return true;
        }

        public async Task<bool> UpdateStaffAsync(Guid id, StaffUpdateDto dto)
        {
            var staff = await _repo.GetByIdAsync(id);
            if (staff == null) return false;

            staff.FullName = dto.FullName;
            staff.PhoneNumber = dto.PhoneNumber;
            staff.BirthDay = dto.BirthDay;
            staff.Gender = dto.Gender;
            staff.IsActive = dto.IsActive;

            await _repo.UpdateAsync(staff);
            return true;
        }

        public async Task<bool> DeleteStaffAsync(Guid id)
        {
            await _repo.DeleteAsync(id);
            return true;
        }

        private StaffResponseDto MapToDto(Staff s) => new StaffResponseDto
        {
            StaffID = s.StaffId,
            FullName = s.FullName,
            Email = s.Email ?? "",
            PhoneNumber = s.PhoneNumber,
            Gender = s.Gender,
            HiredDate = s.HiredDate,
            IsActive = s.IsActive ?? true
        };
    }
}