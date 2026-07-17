using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface IAppointmentRepository
    {
        Task<List<Models.Service>> GetServicesAsync(List<Guid> serviceIds);

        Task<GlobalWorkSchedule?> GetGlobalScheduleAsync(DayOfWeek dayOfWeek);

        Task<ScheduleException?> GetStaffExceptionAsync(
            Guid staffId,
            DateOnly date);

        Task<ScheduleException?> GetGlobalExceptionAsync(
            DateOnly date);

        Task<bool> IsTimeConflictAsync(
            Guid staffId,
            DateTime appointmentStart,
            DateTime appointmentEnd);

        Task<Appointment> CreateAppointmentAsync(Appointment appointment);
        Task<Pet?> GetPetForSnapshotAsync(Guid petId);

        Task<Staff?> GetStaffForSnapshotAsync(Guid staffId);
        Task SaveChangesAsync();

        //Load booking data
        Task<IEnumerable<Staff>> GetActiveVetsAsync();

        //View for customer
        Task<List<Appointment>> GetAppointmentsByCustomerAsync(
        Guid customerId);

        Task<Appointment?> GetAppointmentDetailAsync(
            Guid appointmentId);
        Task<Appointment?> GetByIdAsync(Guid appointmentId);

        Task<List<Appointment>> GetDoctorAppointmentsByDateAsync(
        Guid staffId,
        DateOnly date);

    }
}
