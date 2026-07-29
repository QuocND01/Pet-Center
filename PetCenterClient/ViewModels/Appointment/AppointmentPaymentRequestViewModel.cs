namespace PetCenterClient.ViewModels.Appointment
{
    public class AppointmentPaymentRequestViewModel
    {
        public Guid AppointmentId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string ClientIpAddress { get; set; } = string.Empty;
    }

    public class AppointmentPaymentResponseViewModel
    {
        public Guid PaymentId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string TransactionRef { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentUrl { get; set; } = string.Empty;
    }
}
