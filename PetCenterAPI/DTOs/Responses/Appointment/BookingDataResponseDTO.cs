namespace PetCenterAPI.DTOs.Responses.Appointment
{
    public class BookingDataResponseDTO
    {
        public List<BookingPetDTO> Pets { get; set; } = new();

        public List<BookingStaffDTO> Staffs { get; set; } = new();

        public List<BookingServiceDTO> Services { get; set; } = new();
    }
    public class BookingPetDTO
    {
        public Guid PetId { get; set; }

        public string PetName { get; set; } = string.Empty;
        public string? Species { get; set; }

        public string? Breed { get; set; }

        public string? Gender { get; set; }

        public decimal? Weight { get; set; }
        public string? PetAvatar { get; set; }
    }
    public class BookingStaffDTO
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
    public class BookingServiceDTO
    {
        public Guid ServiceId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Duration { get; set; }

        
        public List<string> ServiceImages { get; set; } = new List<string>();
    }
    public class AvailableSlotDTO
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
