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

        public StaffService(IStaffRepository repo, PasswordService pwService)
        {
            _repo = repo;
            _pwService = pwService;
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
            if (await _repo.EmailExistsAsync(dto.Email)) return false;

            var staff = new Staff
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                BirthDay = dto.BirthDay,
                Gender = dto.Gender,
                HiredDate = DateTime.Now,
                PasswordHash = _pwService.Hash(dto.Password)
            };
            await _repo.AddAsync(staff);
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
            Email = s.Email,
            PhoneNumber = s.PhoneNumber,
            Gender = s.Gender,
            HiredDate = s.HiredDate,
            IsActive = s.IsActive ?? true
        };
    }
}