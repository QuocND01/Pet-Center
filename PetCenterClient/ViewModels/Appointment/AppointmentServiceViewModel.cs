namespace PetCenterClient.ViewModels.Appointment
{
    public class AppointmentServiceViewModel
    {
        public string ServiceName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Duration { get; set; }
    }
    public class ServiceViewModel
    {
        public Guid ServiceId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Duration { get; set; }
    }
    public class PetViewModel
    {
        public Guid PetId { get; set; }

        public string PetName { get; set; } = string.Empty;
    }
    public class StaffViewModel
    {
        public Guid StaffId { get; set; }

        public string FullName { get; set; } = string.Empty;
    }
}
