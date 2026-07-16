using PetCenterAPI.DTOs.Requests.Appointment;
using PetCenterAPI.DTOs.Responses.Appointment;

namespace PetCenterAPI.Service.Interface
{
    public interface IAppointmentService
    {
        Task<AppointmentResponseDTO> BookAppointmentAsync(
        BookAppointmentRequestDTO request);

        Task<BookingDataResponseDTO> GetBookingDataAsync(Guid customerId);
        //View for customer
        Task<List<AppointmentListResponseDTO>>
        GetMyAppointmentsAsync(Guid customerId);

        Task<AppointmentResponseDTO> GetAppointmentDetailAsync(Guid appointmentId);
        Task CancelAppointmentAsync(Guid appointmentId,Guid customerId);
        Task SubmitReviewAsync(Guid customerId,SubmitReviewRequestDTO request);

    }
}
