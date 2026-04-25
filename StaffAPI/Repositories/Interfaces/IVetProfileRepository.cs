// File: Repositories/Interfaces/IVetProfileRepository.cs
using StaffAPI.Models;

namespace StaffAPI.Repositories.Interfaces;

public interface IVetProfileRepository
{
    IQueryable<VetProfile> GetAll();

    Task<VetProfile?> GetByIdAsync(Guid id);

    Task<VetProfile?> GetByStaffIdAsync(Guid staffId);

    Task<VetProfile?> GetByLicenseNumberAsync(string licenseNumber);

    Task<VetProfile> CreateAsync(VetProfile vetProfile);

    Task<VetProfile> UpdateAsync(VetProfile vetProfile);

    Task SoftDeleteAsync(VetProfile vetProfile);
}