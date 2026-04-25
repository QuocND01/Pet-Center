// File: Services/Interfaces/IPrescriptionItemService.cs
using MedicalAPI.DTOs.Prescription;

namespace MedicalAPI.Services.Interfaces;

public interface IPrescriptionItemService
{
    /// <summary>Gets all prescription items for a record.</summary>
    Task<List<PrescriptionItemReadDto>> GetByRecordIdAsync(Guid recordId);

    /// <summary>Gets a single prescription item by ID.</summary>
    Task<PrescriptionItemReadDto> GetByIdAsync(Guid id);

    /// <summary>Adds a prescription item to a record.</summary>
    Task<PrescriptionItemReadDto> CreateAsync(Guid recordId, PrescriptionItemCreateDto dto);

    /// <summary>Updates a prescription item.</summary>
    Task<PrescriptionItemReadDto> UpdateAsync(Guid id, PrescriptionItemUpdateDto dto);

    /// <summary>Deletes a prescription item permanently.</summary>
    Task DeleteAsync(Guid id);
}