namespace IdentityAPI.DTOs.Resquest
{
    public class UpdateCustomerProfileRequestDto
    {
        public string FullName { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public DateOnly? BirthDay { get; set; }

        public string Gender { get; set; } = null!;
    }
}
