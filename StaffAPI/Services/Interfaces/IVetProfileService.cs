// File: Services/Interfaces/IVetProfileService.cs
using StaffAPI.DTOs.VetProfile;

namespace StaffAPI.Services.Interfaces;

public interface IVetProfileService
{
    IQueryable<Models.VetProfile> GetAllQueryable();

    Task<VetProfileReadDto> GetByIdAsync(Guid id);

    Task<VetProfileReadDto> GetByStaffIdAsync(Guid staffId);

    Task<VetProfileReadDto> CreateAsync(Guid staffId, VetProfileCreateDto dto);

    Task<VetProfileReadDto> UpdateAsync(Guid id, VetProfileUpdateDto dto);

    Task SoftDeleteAsync(Guid id);
}