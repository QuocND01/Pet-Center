// File: Services/StaffService.cs
using AutoMapper;
using StaffAPI.DTOs.Staff;
using StaffAPI.Models;
using StaffAPI.Repositories.Interfaces;
using StaffAPI.Services.Interfaces;

namespace StaffAPI.Services;

public class StaffService : IStaffService
{
    private const string RoleNameVet = "vet";

    private readonly IStaffRepository _staffRepo;
    private readonly IVetProfileRepository _vetRepo;
    private readonly IMapper _mapper;

    public StaffService(
        IStaffRepository staffRepo,
        IVetProfileRepository vetRepo,
        IMapper mapper)
    {
        _staffRepo = staffRepo;
        _vetRepo = vetRepo;
        _mapper = mapper;
    }

    
    public IQueryable<Staff> GetAllQueryable()
        => _staffRepo.GetAll();

    
    public async Task<StaffReadDto> GetByIdAsync(Guid id)
    {
        var staff = await _staffRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Staff {id} not found.");
        return _mapper.Map<StaffReadDto>(staff);
    }

    
    public async Task<StaffReadDto> CreateAsync(StaffCreateDto dto)
    {
        // Validate email unique
        var existing = await _staffRepo.GetByEmailAsync(dto.Email);
        if (existing != null)
            throw new InvalidOperationException($"Email '{dto.Email}' is already in use.");

        // Validate roles exist
        var roles = await _staffRepo.GetRolesByIdsAsync(dto.RoleIds);
        if (roles.Count != dto.RoleIds.Count)
            throw new ArgumentException("One or more RoleIds are invalid or inactive.");

        // Check if vet role is assigned
        bool isVet = roles.Any(r => r.RoleName.Equals(RoleNameVet, StringComparison.OrdinalIgnoreCase));
        if (isVet && dto.VetProfile == null)
            throw new ArgumentException("VetProfile information is required when assigning the Vet role.");

        var staff = new Staff
        {
            StaffId = Guid.NewGuid(),
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            BirthDate = dto.BirthDate,
            Gender = dto.Gender,
            HireDate = dto.HireDate,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Avatar = dto.Avatar,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Roles = roles
        };

        var created = await _staffRepo.CreateAsync(staff);

        // Create VetProfile if vet
        if (isVet && dto.VetProfile != null)
        {
            // Check license number uniqueness
            if (!string.IsNullOrWhiteSpace(dto.VetProfile.LicenseNumber))
            {
                var existingLicense = await _vetRepo.GetByLicenseNumberAsync(dto.VetProfile.LicenseNumber);
                if (existingLicense != null)
                    throw new InvalidOperationException($"LicenseNumber '{dto.VetProfile.LicenseNumber}' already exists.");
            }

            var vetProfile = new VetProfile
            {
                VetProfileId = Guid.NewGuid(),
                StaffId = created.StaffId,
                Description = dto.VetProfile.Description,
                LicenseNumber = dto.VetProfile.LicenseNumber,
                ExperienceYears = 0, // starts at 0, accumulates over years
                IsActive = true
            };
            await _vetRepo.CreateAsync(vetProfile);

            // Reload to include VetProfile
            created = await _staffRepo.GetByIdAsync(created.StaffId) ?? created;
        }

        return _mapper.Map<StaffReadDto>(created);
    }

    
    public async Task<StaffReadDto> UpdateAsync(Guid id, StaffUpdateDto dto)
    {
        var staff = await _staffRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Staff {id} not found.");

        if (dto.Email != null && dto.Email != staff.Email)
        {
            var dup = await _staffRepo.GetByEmailAsync(dto.Email);
            if (dup != null)
                throw new InvalidOperationException($"Email '{dto.Email}' is already in use.");
            staff.Email = dto.Email;
        }

        if (dto.FullName != null) staff.FullName = dto.FullName;
        if (dto.PhoneNumber != null) staff.PhoneNumber = dto.PhoneNumber;
        if (dto.BirthDate != null) staff.BirthDate = dto.BirthDate.Value;
        if (dto.Gender != null) staff.Gender = dto.Gender;
        if (dto.HireDate != null) staff.HireDate = dto.HireDate.Value;
        if (dto.Avatar != null) staff.Avatar = dto.Avatar;
        if (dto.IsActive != null) staff.IsActive = dto.IsActive.Value;
        if (dto.Password != null) staff.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        staff.UpdatedAt = DateTime.UtcNow;

        var updated = await _staffRepo.UpdateAsync(staff);
        return _mapper.Map<StaffReadDto>(updated);
    }

    
    public async Task SoftDeleteAsync(Guid id)
    {
        var staff = await _staffRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Staff {id} not found.");
        await _staffRepo.SoftDeleteAsync(staff);
    }

    public async Task<StaffInternalDto?> GetInternalAsync(Guid staffId)
    {
        var staff = await _staffRepo.GetByIdInternalAsync(staffId);
        if (staff == null) return null;
        return new StaffInternalDto { FullName = staff.FullName };
    }
}