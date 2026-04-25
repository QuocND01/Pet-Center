// File: Services/Interfaces/IMedicalRecordService.cs
using MedicalAPI.DTOs.MedicalRecord;

namespace MedicalAPI.Services.Interfaces;

public interface IMedicalRecordService
{
    /// <summary>Returns an IQueryable of ReadDto for OData.</summary>
    IQueryable<MedicalRecordReadDto> GetQueryable();

    /// <summary>Gets a single record by ID.</summary>
    Task<MedicalRecordReadDto> GetByIdAsync(Guid id);

    /// <summary>Gets all records for an appointment.</summary>
    Task<List<MedicalRecordReadDto>> GetByAppointmentIdAsync(Guid appointmentId);

    /// <summary>Creates a new medical record after validating the appointment.</summary>
    Task<MedicalRecordReadDto> CreateAsync(MedicalRecordCreateDto dto);

    /// <summary>Updates diagnosis, treatment, note.</summary>
    Task<MedicalRecordReadDto> UpdateAsync(Guid id, MedicalRecordUpdateDto dto);

    /// <summary>Updates status of a medical record.</summary>
    Task<MedicalRecordReadDto> UpdateStatusAsync(Guid id, MedicalRecordStatusUpdateDto dto);

    /// <summary>Soft-deletes by setting status to Draft.</summary>
    Task DeleteAsync(Guid id);
}