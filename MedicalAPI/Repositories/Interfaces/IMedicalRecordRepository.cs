// File: Repositories/Interfaces/IMedicalRecordRepository.cs
using MedicalAPI.Models;

namespace MedicalAPI.Repositories.Interfaces;

public interface IMedicalRecordRepository
{
    /// <summary>Returns an IQueryable for OData filtering/paging.</summary>
    IQueryable<MedicalRecord> GetQueryable();

    /// <summary>Gets a record by ID including prescriptions.</summary>
    Task<MedicalRecord?> GetByIdAsync(Guid id);

    /// <summary>Gets all records by appointmentId.</summary>
    Task<List<MedicalRecord>> GetByAppointmentIdAsync(Guid appointmentId);

    /// <summary>Creates a new medical record.</summary>
    Task<MedicalRecord> CreateAsync(MedicalRecord record);

    /// <summary>Updates an existing medical record.</summary>
    Task<MedicalRecord> UpdateAsync(MedicalRecord record);

    /// <summary>Soft-deletes a record by setting status to Draft (0).</summary>
    Task<MedicalRecord> SoftDeleteAsync(MedicalRecord record);
}