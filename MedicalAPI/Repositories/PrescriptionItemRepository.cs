// File: Repositories/PrescriptionItemRepository.cs
using MedicalAPI.Models;
using MedicalAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedicalAPI.Repositories;

public class PrescriptionItemRepository : IPrescriptionItemRepository
{
    private readonly PetCenterMedicalServiceContext _context;

    public PrescriptionItemRepository(PetCenterMedicalServiceContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<List<PrescriptionItem>> GetByRecordIdAsync(Guid recordId)
        => await _context.PrescriptionItems
            .Where(p => p.RecordId == recordId)
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<PrescriptionItem?> GetByIdAsync(Guid id)
        => await _context.PrescriptionItems
            .Include(p => p.Record)
            .FirstOrDefaultAsync(p => p.PrescriptionItemId == id);

    /// <inheritdoc/>
    public async Task<PrescriptionItem> CreateAsync(PrescriptionItem item)
    {
        _context.PrescriptionItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    /// <inheritdoc/>
    public async Task CreateRangeAsync(IEnumerable<PrescriptionItem> items)
    {
        _context.PrescriptionItems.AddRange(items);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<PrescriptionItem> UpdateAsync(PrescriptionItem item)
    {
        _context.PrescriptionItems.Update(item);
        await _context.SaveChangesAsync();
        return item;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(PrescriptionItem item)
    {
        _context.PrescriptionItems.Remove(item);
        await _context.SaveChangesAsync();
    }
}