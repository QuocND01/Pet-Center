// File: Services/Interfaces/IStaffService.cs
using StaffAPI.DTOs.Staff;

namespace StaffAPI.Services.Interfaces;

public interface IStaffService
{
    /// <summary>OData-aware queryable list.</summary>
    IQueryable<Models.Staff> GetAllQueryable();

    Task<StaffReadDto> GetByIdAsync(Guid id);

    Task<StaffReadDto> CreateAsync(StaffCreateDto dto);

    Task<StaffReadDto> UpdateAsync(Guid id, StaffUpdateDto dto);

    Task SoftDeleteAsync(Guid id);

    Task<StaffInternalDto?> GetInternalAsync(Guid staffId);
}