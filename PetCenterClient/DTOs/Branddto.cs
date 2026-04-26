using PetCenterClient.Models;
using System.ComponentModel.DataAnnotations;

namespace PetCenterClient.DTOs
{
    public class ReadBrandDTOs
    {

        public Guid BrandId { get; set; }
        public string BrandName { get; set; } = null!;
        public string? BrandDescription { get; set; }

        public string? BrandLogo { get; set; }
    }

    public class UpdateBrandDTOs
    {


        [Required(ErrorMessage = "Brand name is required")]
        [MaxLength(200, ErrorMessage = "Brand name cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$",
           ErrorMessage = "Brand name cannot contain special characters")]
        public string BrandName { get; set; } = null!;
        public string? BrandDescription { get; set; }
        public IFormFile? BrandLogo { get; set; }
    }

    public class CreateBrandDTOs
    {
        [Required(ErrorMessage = "Brand name is required")]
        [MaxLength(200, ErrorMessage = "Brand name cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$",
           ErrorMessage = "Brand name cannot contain special characters")]
        public string BrandName { get; set; } = null!;
        public string? BrandDescription { get; set; }

        public IFormFile? BrandLogo { get; set; }
    }
}
