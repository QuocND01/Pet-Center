namespace PetCenterAPI.DTOs.Responses.Appointment
{
    public class AppointmentCallbackResponseDTO
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? AppointmentId { get; set; }
        public string TransactionRef { get; set; } = string.Empty;
    }
    public class PaymentCallbackResponseDTO
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? AppointmentId { get; set; }
        public string TransactionRef { get; set; } = string.Empty;
    }
}
