using System.Text.Json.Serialization;

namespace PetCenterClient.ViewModels.ManageCustomer
{
    public class CustomerListViewModel
    {
        public Guid CustomerId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public bool IsActive { get; set; }
    }

    public class CustomerDetailViewModel
    {
        public Guid CustomerId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public DateTime? BirthDay { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        
        [JsonPropertyName("isVerified")]
        public bool EmailVerified { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ChangeCustomerStatusViewModel
    {
        public bool IsActive { get; set; }
    }
}
