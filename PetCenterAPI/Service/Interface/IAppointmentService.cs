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
        Task<List<AppointmentListResponseDTO>> GetMyAppointmentsAsync(Guid customerId);
        Task<List<AppointmentListResponseDTO>> GetAllAppointmentsAsync();

        Task<AppointmentResponseDTO> GetAppointmentDetailAsync(Guid appointmentId);
        Task CancelAppointmentAsync(Guid appointmentId,Guid customerId);
        Task ForwardAppointmentStatusAsync(Guid appointmentId,Guid staffId);
        Task SubmitReviewAsync(Guid customerId,SubmitReviewRequestDTO request);
        Task CompleteAppointmentService(Guid AppointmentServiceId);
        Task<List<AvailableSlotResponseDTO>>GetAvailableSlotsAsync(GetAvailableSlotsRequestDTO request);

    }
}
