using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Requests.Appointment
{
    public class AppointmentPaymentRequestDTO
    {
        [Required]
        public Guid AppointmentId { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = null!;
        public string ClientIpAddress { get; set; } = string.Empty;
    }
}
