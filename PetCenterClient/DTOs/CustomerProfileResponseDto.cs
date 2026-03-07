namespace PetCenterClient.DTOs
{
    public class CustomerProfileResponseDto
    {
        public Guid CustomerId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime BirthDay { get; set; }
        public string Gender { get; set; }
        public bool EmailVerified { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
