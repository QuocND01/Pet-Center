namespace PetCenterClient.DTOs
{
    public class CustomerProfileResponseDto
    {
        public Guid CustomerId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateOnly? BirthDay { get; set; }
        public string Gender { get; set; }
        public bool? EmailVerified { get; set; }  // ← nullable
        public bool? IsActive { get; set; }       // ← nullable
        public DateTime? CreatedAt { get; set; }
    }
}
