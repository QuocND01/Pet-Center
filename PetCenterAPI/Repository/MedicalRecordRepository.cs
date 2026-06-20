using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Common;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;

namespace PetCenterAPI.Repository
{
    public class MedicalRecordRepository : IMedicalRecordRepository
    {
        private readonly PetCenterContext _db;

        public MedicalRecordRepository(PetCenterContext db)
        {
            _db = db;
        }

        public async Task<(IEnumerable<MedicalRecord> Items, int Total)> GetAllAsync(
            string? search, int? status, int page, int pageSize)
        {
            var query = _db.MedicalRecords
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Customer)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.AppointmentSnapshot)
                .Include(r => r.PrescriptionItems)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(r =>
                    r.Diagnosis.ToLower().Contains(search) ||
                    r.Treatment.ToLower().Contains(search) ||
                    r.Appointment.Customer.FullName!.ToLower().Contains(search));
            }

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<IEnumerable<MedicalRecord>> GetByCustomerIdAsync(Guid customerId, string? search)
        {
            var query = _db.MedicalRecords
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Customer)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.AppointmentSnapshot)
                .Include(r => r.PrescriptionItems)
                .Where(r => r.Appointment.CustomerId == customerId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(r =>
                    r.Diagnosis.ToLower().Contains(search) ||
                    r.Treatment.ToLower().Contains(search));
            }

            return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        }

        public async Task<MedicalRecord?> GetByIdAsync(Guid id)
        {
            return await _db.MedicalRecords
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Customer)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.AppointmentSnapshot)
                .Include(r => r.PrescriptionItems)
                .FirstOrDefaultAsync(r => r.RecordId == id);
        }

        public async Task<IEnumerable<Appointment>> GetCompletedAppointmentsAsync()
        {
            // Status 4 = Completed appointment
            return await _db.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Pet)
                .Include(a => a.AppointmentSnapshot)
                .Where(a => a.Status == 4)
                .OrderByDescending(a => a.AppointmentStart)
                .ToListAsync();
        }

        public async Task AddAsync(MedicalRecord record)
        {
            _db.MedicalRecords.Add(record);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(MedicalRecord record)
        {
            _db.MedicalRecords.Update(record);
            await _db.SaveChangesAsync();
        }

        public async Task ChangeStatusAsync(Guid id, MedicalRecordStatus status)
        {
            await _db.MedicalRecords
                .Where(r => r.RecordId == id)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, (int)status));
        }
    }
}
