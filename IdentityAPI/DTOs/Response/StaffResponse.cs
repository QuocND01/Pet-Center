namespace IdentityAPI.DTOs.Response
{
    public class StaffResponseDto
    {
        public Guid StaffID { get; set; }
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Gender { get; set; }
        public DateTime? BirthDay { get; set; }
        public DateTime? HiredDate { get; set; }
        public bool IsActive { get; set; }
    }
}