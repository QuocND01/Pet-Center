namespace PetCenterAPI.DTOs.Requests.Appointment
{
    public class SubmitReviewRequestDTO
    {
        public Guid AppointmentId { get; set; }

        public decimal Rating { get; set; }

        public string? Feedback { get; set; }
    }
}
