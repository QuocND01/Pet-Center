using System.ComponentModel.DataAnnotations;

namespace CustomerAPI.DTOs.Request
{
    public class ChangeCustomerStatusRequestDto
    {
        [Required]
        public bool IsActive { get; set; }
    }
}
