namespace CustomerAPI.DTOs.Response
{
    public class CustomerResponseDto
    {
        public Guid CustomerId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateOnly? BirthDay { get; set; }
        public string? Gender { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
