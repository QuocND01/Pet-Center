using PetCenterAPI.DTOs.Requests.Appointment;
using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface IAppointmentRepository
    {
        // ==================== QUERY METHODS ====================
        Task<Appointment?> GetByIdAsync(Guid appointmentId);

        /// <summary>
        /// Lấy Appointment kèm theo AppointmentServices VÀ AppointmentSnapshot để phục vụ Update.
        /// Bắt buộc có Tracking trong Change Tracker.
        /// </summary>
        Task<Appointment?> GetByIdForUpdateAsync(Guid appointmentId);

        Task<Appointment?> GetAppointmentDetailAsync(Guid appointmentId);

        Task<List<Appointment>> GetAppointmentsByCustomerAsync(Guid customerId);

        Task<List<Appointment>> GetAllAppointmentsAsync();

        Task<List<Appointment>> GetDoctorAppointmentsByDateAsync(Guid staffId, DateOnly date);
        Task<bool> IsPetTimeConflictAsync(Guid petId, DateTime start, DateTime end);

        // ==================== SNAPSHOT & AUXILIARY DATA ====================
        Task<Pet?> GetPetForSnapshotAsync(Guid petId);

        Task<Staff?> GetStaffForSnapshotAsync(Guid staffId);

        Task<IEnumerable<Staff>> GetActiveVetAndGroomersAsync();

        Task<List<Models.Service>> GetServicesAsync(List<Guid> serviceIds);

        Task<AppointmentService?> GetAppointmentServiceByIdAsync(Guid appointmentServiceId);

        // ==================== SCHEDULE & EXCEPTION CHECKS ====================
        Task<ScheduleException?> GetStaffExceptionAsync(Guid staffId, DateOnly date);

        Task<ScheduleException?> GetGlobalExceptionAsync(DateOnly date);

        Task<GlobalWorkSchedule?> GetGlobalScheduleAsync(DayOfWeek dayOfWeek);

        Task<bool> IsTimeConflictAsync(Guid staffId, DateTime appointmentStart, DateTime appointmentEnd);
        Task<int> GetActiveAppointmentsCountByCustomerAsync(Guid customerId);

        // ==================== COMMAND / PERSISTENCE ====================
        Task<Appointment> CreateAppointmentAsync(Appointment appointment);

        Task UpdateAsync(Appointment appointment);

        Task SaveChangesAsync();
        //Backgournd check
        Task<int> UpdateExpiredAppointmentsAsync(CancellationToken cancellationToken = default);
    }
}
