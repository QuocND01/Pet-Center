namespace PetCenterClient.ViewModels.Appointment
{
    public class BookingPageViewModel
    {
        public Guid? PetId { get; set; }

        public Guid? StaffId { get; set; }

        public List<Guid> ServiceIds { get; set; } = new();

        public DateTime AppointmentStart { get; set; }

        public string? Note { get; set; }

        public List<BookingPetViewModel> Pets { get; set; } = new();

        public List<BookingStaffViewModel> Staffs { get; set; } = new();

        public List<BookingServiceViewModel> Services { get; set; } = new();
    }
    public class BookingPetViewModel
    {
        public Guid PetId { get; set; }

        public string PetName { get; set; } = string.Empty;
        public string? Species { get; set; }

        public string? Breed { get; set; }

        public string? Gender { get; set; }

        public decimal? Weight { get; set; }
        public string? PetAvatar { get; set; }
    }
    public class BookingStaffViewModel
    {
        public Guid StaffId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Thông tin bổ sung từ VetProfile
        public decimal? ExperienceYears { get; set; }
        public string? Description { get; set; }
        public string? LicenseNumber { get; set; }
    }
    public class BookingServiceViewModel
    {
        public Guid ServiceId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Duration { get; set; }
        public int ServiceType { get; set; }

        public List<string> ServiceImages { get; set; } = new List<string>();
    }
}
