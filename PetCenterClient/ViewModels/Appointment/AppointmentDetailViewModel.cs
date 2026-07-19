namespace PetCenterClient.ViewModels.Appointment
{
    public class AppointmentDetailViewModel
    
    {
        public Guid AppointmentId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string PetName { get; set; } = string.Empty;
        public string PetAvatar { get; set; } = string.Empty;
        public string VetName { get; set; } = string.Empty;
        public string VetAvatar { get; set; } = string.Empty;
        public DateTime AppointmentStart { get; set; }

        public DateTime AppointmentEnd { get; set; }

        public decimal Total { get; set; }

        public int Status { get; set; }

        public string? Note { get; set; }

        public List<ServiceDetailViewModel> AppointmentServices { get; set; } = new();
        public AppointmentSnapshotViewModel? Snapshot { get; set; }
    }
    public class ServiceDetailViewModel
    {
        public Guid ServiceId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Duration { get; set; }

        public int? Status { get; set; }
        public int ServiceType { get; set; }
        public DateTime? CompleteAt { get; set; }
    }
    public class AppointmentSnapshotViewModel
    {
        public string Species { get; set; } = string.Empty;

        public string Breed { get; set; } = string.Empty;

        public string Gender { get; set; } = string.Empty;

        public decimal Weight { get; set; }

        public string? Feedback { get; set; }

        public decimal Rating { get; set; }

        public string VetName { get; set; } = string.Empty;
    }
}
