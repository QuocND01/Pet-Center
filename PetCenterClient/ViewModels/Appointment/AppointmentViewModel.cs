namespace PetCenterClient.ViewModels.Appointment
{
    public class AppointmentViewModel
    {
        public Guid AppointmentId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string PetName { get; set; } = string.Empty;

        public string VetName { get; set; } = string.Empty;

        public DateTime AppointmentStart { get; set; }

        public DateTime AppointmentEnd { get; set; }

        public decimal Total { get; set; }

        public int Status { get; set; } 

        public string? Note { get; set; }

        public List<AppointmentServiceViewModel> Services { get; set; } = new();
    }
}
