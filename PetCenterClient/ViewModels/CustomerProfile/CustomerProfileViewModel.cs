using System.Text.Json.Serialization;

namespace PetCenterClient.ViewModels.CustomerProfile
{
    public class CustomerProfileViewModel
    {
        [JsonPropertyName("customerId")]
        public Guid CustomerId { get; set; }

        [JsonPropertyName("fullName")]
        public string? FullName { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("birthDay")]
        public DateOnly? BirthDay { get; set; }

        [JsonPropertyName("gender")]
        public string? Gender { get; set; }

        [JsonPropertyName("isVerified")]
        public bool? IsVerified { get; set; }

        [JsonPropertyName("isActive")]
        public bool? IsActive { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime? CreatedAt { get; set; }
    }

    public class UpdateCustomerProfileViewModel
    {
        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = null!;

        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; } = null!;

        [JsonPropertyName("birthDay")]
        public DateTime BirthDay { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; } = null!;
    }
}
