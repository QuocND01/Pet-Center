namespace PetCenterAPI.DTOs.Responses.Appointment
{
    public class AppointmentResponseDTO
    {
        public Guid AppointmentId { get; set; }

        public Guid CustomerId { get; set; }

        public Guid PetId { get; set; }

        public Guid StaffId { get; set; }

        public DateTime AppointmentStart { get; set; }

        public DateTime AppointmentEnd { get; set; }

        public decimal Total { get; set; }

        public int Status { get; set; }

        public string? Note { get; set; }

        public DateTime? CreatedAt { get; set; }

        public List<AppointmentServiceResponseDTO> AppointmentServices { get; set; }
    = new();

        public AppointmentSnapshotResponseDTO? Snapshot { get; set; }
    }
    public class AppointmentServiceResponseDTO
    {
        public Guid ServiceId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Duration { get; set; }

        public int ServiceType { get; set; }
    }
    public class AppointmentSnapshotResponseDTO
    {
        public string Species { get; set; } = string.Empty;

        public string Breed { get; set; } = string.Empty;

        public string Gender { get; set; } = string.Empty;

        public decimal Weight { get; set; }

        public decimal ExperienceYears { get; set; }

        public decimal Rating { get; set; }

        public string VetName { get; set; } = string.Empty;
    }
}
