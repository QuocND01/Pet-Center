using System.Text.Json.Serialization;

namespace PetCenterClient.DTOs
{
    public class UpdateCustomerProfileRequestDto
    {
        [JsonPropertyName("fullName")]
        public string FullName { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonPropertyName("birthDay")]
        public DateTime BirthDay { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; }
    }
}
