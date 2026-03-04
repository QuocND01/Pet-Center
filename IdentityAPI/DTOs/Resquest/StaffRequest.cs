namespace IdentityAPI.DTOs.Request
{
    public class StaffCreateDto
    {
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateTime? BirthDay { get; set; }
        public string? Gender { get; set; }
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!; // Để hash khi lưu
    }

    public class StaffUpdateDto
    {
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateTime? BirthDay { get; set; }
        public string? Gender { get; set; }
        public bool IsActive { get; set; }
    }
}