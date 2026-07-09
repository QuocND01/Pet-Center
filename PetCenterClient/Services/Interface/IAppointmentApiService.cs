using PetCenterClient.ViewModels.Appointment;   

namespace PetCenterClient.Services.Interface
{
    public interface IAppointmentApiService
    {
        Task<AppointmentViewModel?> BookAppointmentAsync(
            BookAppointmentViewModel request);

        Task<BookingPageViewModel?> GetBookingDataAsync();
    }
}
