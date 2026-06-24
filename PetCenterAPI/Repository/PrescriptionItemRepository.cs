using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;

namespace PetCenterAPI.Repository
{
    public class PrescriptionItemRepository : IPrescriptionItemRepository
    {
        private readonly PetCenterContext _db;

        public PrescriptionItemRepository(PetCenterContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<PrescriptionItem>> GetByRecordIdAsync(Guid recordId)
        {
            return await _db.PrescriptionItems
                .Where(p => p.RecordId == recordId)
                .OrderBy(p => p.MedicineName)
                .ToListAsync();
        }

        public async Task<PrescriptionItem?> GetByIdAsync(Guid id)
        {
            return await _db.PrescriptionItems
                .FirstOrDefaultAsync(p => p.PrescriptionItemId == id);
        }

        public async Task<int?> GetRecordStatusAsync(Guid recordId)
        {
            return await _db.MedicalRecords
                .Where(r => r.RecordId == recordId)
                .Select(r => r.Status)
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(PrescriptionItem item)
        {
            _db.PrescriptionItems.Add(item);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(PrescriptionItem item)
        {
            _db.PrescriptionItems.Update(item);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            await _db.PrescriptionItems
                .Where(p => p.PrescriptionItemId == id)
                .ExecuteDeleteAsync();
        }
    }
}
