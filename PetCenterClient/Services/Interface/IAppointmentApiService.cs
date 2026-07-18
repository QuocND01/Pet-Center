using PetCenterClient.ViewModels.Appointment;   

namespace PetCenterClient.Services.Interface
{
    public interface IAppointmentApiService
    {
        Task<AppointmentViewModel?> BookAppointmentAsync(
            BookAppointmentViewModel request);

        Task<BookingPageViewModel?> GetBookingDataAsync();
        Task<List<AvailableSlotViewModel>> GetAvailableSlotsAsync(
    Guid doctorId,
    DateOnly date,
    List<Guid> serviceIds);
    }
}
