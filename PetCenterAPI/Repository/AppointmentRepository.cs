using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;

namespace PetCenterAPI.Repository
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly PetCenterContext _context;

        public AppointmentRepository(PetCenterContext context)
        {
            _context = context;
        }

        public async Task<List<Models.Service>> GetServicesAsync(List<Guid> serviceIds)
        {
            return await _context.Services
                .Where(s => serviceIds.Contains(s.ServiceId))
                .ToListAsync();
        }

        public async Task<ScheduleException?> GetStaffExceptionAsync(Guid staffId, DateOnly date)
        {
            return await _context.ScheduleExceptions
                .FirstOrDefaultAsync(x => x.StaffId == staffId && x.ExceptionDate == date);
        }

        public async Task<ScheduleException?> GetGlobalExceptionAsync(DateOnly date)
        {
            return await _context.ScheduleExceptions
                .FirstOrDefaultAsync(x => x.StaffId == null && x.ExceptionDate == date);
        }

        public async Task<GlobalWorkSchedule?> GetGlobalScheduleAsync(DayOfWeek dayOfWeek)
        {
            int day = dayOfWeek == DayOfWeek.Sunday ? 7 : (int)dayOfWeek;

            return await _context.GlobalWorkSchedules
                .FirstOrDefaultAsync(x => x.DayOfWeek == day);
        }

        public async Task<bool> IsTimeConflictAsync(Guid staffId, DateTime appointmentStart, DateTime appointmentEnd)
        {
            // Kiểm tra trùng lịch nhân viên đối với các lịch đang Active (1: Reserved, 2: Confirmed)
            return await _context.Appointments.AnyAsync(x =>
                x.StaffId == staffId
                && appointmentStart < x.AppointmentEnd
                && appointmentEnd > x.AppointmentStart
                && (x.Status == 1 || x.Status == 2)
            );
        }

        public async Task<int> GetActiveAppointmentsCountByCustomerAsync(Guid customerId)
        {
            var now = DateTime.Now;

            return await _context.Appointments
                .CountAsync(a => a.CustomerId == customerId
                              && (a.Status == 1 || a.Status == 2)
                              && a.AppointmentStart > now);
        }

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            var entry = await _context.Appointments.AddAsync(appointment);
            return entry.Entity;
        }

        public async Task<Pet?> GetPetForSnapshotAsync(Guid petId)
        {
            return await _context.Pets
                .FirstOrDefaultAsync(x => x.PetId == petId);
        }

        public async Task<Staff?> GetStaffForSnapshotAsync(Guid staffId)
        {
            return await _context.Staffs
                .Include(s => s.VetProfile)
                .FirstOrDefaultAsync(x => x.StaffId == staffId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Staff>> GetActiveVetAndGroomersAsync()
        {
            return await _context.Staffs
                .Include(s => s.VetProfile)
                .Include(s => s.Roles)
                .Where(s =>
                    s.IsActive &&
                    s.Roles.Any(r =>
                        r.IsActive &&
                        (r.RoleName == "Vet" || r.RoleName == "Groomer")))
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetAppointmentsByCustomerAsync(Guid customerId)
        {
            return await _context.Appointments
                .Include(x => x.Pet)
                .Include(x => x.Staff)
                .Where(x => x.CustomerId == customerId)
                .OrderByDescending(x => x.AppointmentStart)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetAllAppointmentsAsync()
        {
            return await _context.Appointments
                .Include(x => x.Pet)
                .Include(x => x.Staff)
                .OrderByDescending(x => x.AppointmentStart)
                .ToListAsync();
        }

        public async Task<Appointment?> GetAppointmentDetailAsync(Guid appointmentId)
        {
            return await _context.Appointments
                .Include(x => x.Pet)
                .Include(x => x.Staff)
                .Include(x => x.Customer)
                .Include(x => x.AppointmentSnapshot)
                .Include(x => x.AppointmentServices)
                .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);
        }

        public async Task<AppointmentService?> GetAppointmentServiceByIdAsync(Guid appointmentServiceId)
        {
            return await _context.AppointmentServices
                .FirstOrDefaultAsync(x => x.AppointmentServiceId == appointmentServiceId);
        }

        public async Task<Appointment?> GetByIdAsync(Guid appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.AppointmentServices)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
        }

        public async Task<List<Appointment>> GetDoctorAppointmentsByDateAsync(Guid staffId, DateOnly date)
        {
            var startDate = date.ToDateTime(TimeOnly.MinValue);
            var endDate = startDate.AddDays(1);

            return await _context.Appointments
                .Where(a =>
                    a.StaffId == staffId &&
                    a.AppointmentStart >= startDate &&
                    a.AppointmentStart < endDate &&
                    (a.Status == 1 || a.Status == 2))
                .OrderBy(a => a.AppointmentStart)
                .ToListAsync();
        }

        public async Task UpdateAsync(Appointment appointment)
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Appointment?> GetByIdForUpdateAsync(Guid appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.AppointmentServices)
                .Include(a => a.AppointmentSnapshot)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
        }

        public async Task<bool> IsPetTimeConflictAsync(Guid petId, DateTime start, DateTime end)
        {
            return await _context.Appointments
                .AnyAsync(a => a.PetId == petId
                            && (a.Status == 1 || a.Status == 2)
                            && a.AppointmentStart < end
                            && a.AppointmentEnd > start);
        }

        public async Task<int> UpdateExpiredAppointmentsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now; // Sử dụng giờ hệ thống Local Time

            return await _context.Appointments
                .Where(a => a.Status == 1
                         && a.ReservedUntil.HasValue
                         && a.ReservedUntil.Value <= now)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(a => a.Status, 0)
                    .SetProperty(a => a.UpdatedAt, now),
                    cancellationToken);
        }
    }
}