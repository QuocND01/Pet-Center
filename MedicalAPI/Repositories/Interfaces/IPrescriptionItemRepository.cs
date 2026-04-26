// File: Repositories/Interfaces/IPrescriptionItemRepository.cs
using MedicalAPI.Models;

namespace MedicalAPI.Repositories.Interfaces;

public interface IPrescriptionItemRepository
{
    /// <summary>Gets all prescriptions by recordId.</summary>
    Task<List<PrescriptionItem>> GetByRecordIdAsync(Guid recordId);

    /// <summary>Gets a prescription by ID.</summary>
    Task<PrescriptionItem?> GetByIdAsync(Guid id);

    /// <summary>Creates a new prescription item.</summary>
    Task<PrescriptionItem> CreateAsync(PrescriptionItem item);

    /// <summary>Creates multiple prescription items in bulk.</summary>
    Task CreateRangeAsync(IEnumerable<PrescriptionItem> items);

    /// <summary>Updates an existing prescription item.</summary>
    Task<PrescriptionItem> UpdateAsync(PrescriptionItem item);

    /// <summary>Deletes a prescription item permanently.</summary>
    Task DeleteAsync(PrescriptionItem item);
}