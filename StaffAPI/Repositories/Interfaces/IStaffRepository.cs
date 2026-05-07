// File: Repositories/Interfaces/IStaffRepository.cs
using StaffAPI.Models;

namespace StaffAPI.Repositories.Interfaces;

public interface IStaffRepository
{
    /// <summary>Returns IQueryable for OData.</summary>
    IQueryable<Staff> GetAll();

    Task<Staff?> GetByIdAsync(Guid id);

    Task<Staff?> GetByEmailAsync(string email);

    Task<Staff> CreateAsync(Staff staff);

    Task<Staff> UpdateAsync(Staff staff);

    /// <summary>SoftDelete: set IsActive = false.</summary>
    Task SoftDeleteAsync(Staff staff);

    Task<bool> ExistsAsync(Guid id);

    Task<List<Models.Role>> GetRolesByIdsAsync(List<Guid> roleIds);

    Task<Staff?> GetByIdInternalAsync(Guid staffId);
}