using PetCenterAPI.DTOs.Requests.Appointment;
using PetCenterAPI.DTOs.Responses.Appointment;

namespace PetCenterAPI.Service.Interface
{
    public interface IAppointmentService
    {
        Task<AppointmentResponseDTO> BookAppointmentAsync(
        BookAppointmentRequestDTO request);

        Task<BookingDataResponseDTO> GetBookingDataAsync(Guid customerId);
    }
}
