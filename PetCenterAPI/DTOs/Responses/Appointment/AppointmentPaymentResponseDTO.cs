namespace PetCenterAPI.DTOs.Responses.Appointment
{
    public class AppointmentPaymentResponseDTO
    {
        public Guid PaymentId { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public string TransactionRef { get; set; } = null!;
        public decimal Amount { get; set; }
        public string PaymentUrl { get; set; } = null!;
    }
}
