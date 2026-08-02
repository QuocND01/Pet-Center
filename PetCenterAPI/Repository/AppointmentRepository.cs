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
            return await _context.Appointments.AnyAsync(x =>
                x.StaffId == staffId
                && appointmentStart < x.AppointmentEnd
                && appointmentEnd > x.AppointmentStart
                && x.Status != 0
            );
        }
        public async Task<int> GetActiveAppointmentsCountByCustomerAsync(Guid customerId)
        {
            // Các trạng thái được tính là lịch hẹn đang hoạt động/chờ xử lý:
            // Status 1: Reserved / Pending
            // Status 2: Confirmed / Paid
            // Status 3: In Progress
            // (Bỏ qua Status 0: Cancelled, 4: Completed, 5: Expired)
            return await _context.Appointments
                .CountAsync(a => a.CustomerId == customerId
                              && (a.Status == 1 || a.Status == 2 || a.Status == 3)
                              && a.AppointmentStart > DateTime.Now);
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

        public async Task<IEnumerable<Staff>> GetActiveVetsAsync()
        {
            return await _context.Staffs
                .Include(s => s.VetProfile)
                .Where(s => s.IsActive && s.VetProfile != null && s.VetProfile.IsActive)
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
                    (a.Status == 1 || a.Status == 2 || a.Status == 3))
                .OrderBy(a => a.AppointmentStart)
                .ToListAsync();
        }

        public async Task UpdateAsync(Appointment appointment)
        {
            // Do Entity đã được Track trong DbContext từ trước,
            // ta chỉ cần gọi SaveChangesAsync() để EF Core tự sinh SQL UPDATE.
            await _context.SaveChangesAsync();
        }

        // HÀM QUAN TRỌNG NHẤT CHO FEATURE UPDATE
        public async Task<Appointment?> GetByIdForUpdateAsync(Guid appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.AppointmentServices)
                .Include(a => a.AppointmentSnapshot)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
        }
        public async Task<bool> IsPetTimeConflictAsync(Guid petId, DateTime start, DateTime end)
        {
            // Giả sử Status: 1 = Pending/Confirmed, các status đã Cancelled/Completed thì không tính trùng
            // Bạn có thể điều chỉnh danh sách Status tùy theo Logic của dự án (ví dụ: status != 3 với 3 là Cancelled)
            return await _context.Appointments
                .AnyAsync(a => a.PetId == petId
                            && a.Status != 0
                            && a.Status != 5
                            && a.AppointmentStart < end
                            && a.AppointmentEnd > start);
        }
    }
}