using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Requests.ManageCustomer
{
    public class ChangeCustomerStatusRequestDTO
    {
        [Required]
        public bool IsActive { get; set; }
    }
}
