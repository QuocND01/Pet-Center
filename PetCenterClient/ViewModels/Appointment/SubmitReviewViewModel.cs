namespace PetCenterClient.ViewModels.Appointment
{
    public class SubmitReviewViewModel
    {
        public Guid AppointmentId { get; set; }

        public decimal Rating { get; set; }

        public string? Feedback { get; set; }
    }
}
