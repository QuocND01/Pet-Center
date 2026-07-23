using PetCenterClient.ViewModels.Appointment;

namespace PetCenterClient.Services.Interface
{
    public interface IAppointmentApiService
    {
        Task<AppointmentBookingViewModel?> BookAppointmentAsync(
            BookAppointmentViewModel request);

        Task<BookingPageViewModel?> GetBookingDataAsync();
        Task<List<AvailableSlotViewModel>> GetAvailableSlotsAsync(
    Guid doctorId,
    DateOnly date,
    List<Guid> serviceIds);
        Task<List<AppointmentListViewModel>> GetMyAppointmentsAsync();

        Task<AppointmentDetailViewModel?> GetAppointmentDetailAsync(Guid appointmentId);

        Task CancelAppointmentAsync(Guid appointmentId);

        Task SubmitReviewAsync(SubmitReviewViewModel model);

        Task<List<AppointmentListViewModel>> GetAllAppointmentsAsync();

        Task ForwardAppointmentStatusAsync(Guid appointmentId);

        Task CompleteAppointmentServiceAsync(Guid appointmentServiceId);
    }
}
