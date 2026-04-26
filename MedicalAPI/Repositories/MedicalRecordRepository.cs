// File: Repositories/MedicalRecordRepository.cs
using MedicalAPI.Models;
using MedicalAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedicalAPI.Repositories;

public class MedicalRecordRepository : IMedicalRecordRepository
{
    private readonly PetCenterMedicalServiceContext _context;

    public MedicalRecordRepository(PetCenterMedicalServiceContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public IQueryable<MedicalRecord> GetQueryable()
        => _context.MedicalRecords
            .Include(r => r.PrescriptionItems)
            .AsQueryable();

    /// <inheritdoc/>
    public async Task<MedicalRecord?> GetByIdAsync(Guid id)
        => await _context.MedicalRecords
            .Include(r => r.PrescriptionItems)
            .FirstOrDefaultAsync(r => r.RecordId == id);

    /// <inheritdoc/>
    public async Task<List<MedicalRecord>> GetByAppointmentIdAsync(Guid appointmentId)
        => await _context.MedicalRecords
            .Include(r => r.PrescriptionItems)
            .Where(r => r.AppointmentId == appointmentId)
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<MedicalRecord> CreateAsync(MedicalRecord record)
    {
        _context.MedicalRecords.Add(record);
        await _context.SaveChangesAsync();
        return record;
    }

    /// <inheritdoc/>
    public async Task<MedicalRecord> UpdateAsync(MedicalRecord record)
    {
        _context.MedicalRecords.Update(record);
        await _context.SaveChangesAsync();
        return record;
    }

    /// <inheritdoc/>
    public async Task<MedicalRecord> SoftDeleteAsync(MedicalRecord record)
    {
        record.Status = (int)MedicalRecordStatus.Draft;
        _context.MedicalRecords.Update(record);
        await _context.SaveChangesAsync();
        return record;
    }
}