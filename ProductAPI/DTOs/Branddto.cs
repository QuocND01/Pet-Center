using ProductAPI.Common;
using ProductAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace ProductAPI.DTOs
{
    public class ReadBrandDTO
    {
        public Guid BrandId { get; set; }
        public string BrandName { get; set; } = null!;
        public string? BrandDescription { get; set; }

        public string? BrandLogo { get; set; }

        public Status Status { get; set; }
    }


    public class ReadBrandDTOForCustomer
    {
        public Guid BrandId { get; set; }
        public string BrandName { get; set; } = null!;
        public string? BrandDescription { get; set; }

        public string? BrandLogo { get; set; }
    }

    public class UpdateBrandDTO
    {

        [Required(ErrorMessage = "Brand name is required")]
        [MaxLength(200, ErrorMessage = "Brand name cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$",
           ErrorMessage = "Brand name cannot contain special characters")]
        public string BrandName { get; set; } = null!;
        public string? BrandDescription { get; set; }
        public IFormFile? BrandLogo { get; set; }
    }

    public class CreateBrandDTO
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
