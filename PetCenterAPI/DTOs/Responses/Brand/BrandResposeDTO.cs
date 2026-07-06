using System.ComponentModel.DataAnnotations;


namespace PetCenterAPI.DTOs.Responses.Brand
{
    public class BrandResposeDTO
    {

        public class UpdateBrandDTO
        {

            [Required(ErrorMessage = "Brand name is required")]
            [MaxLength(200, ErrorMessage = "Brand name cannot exceed 200 characters")]
            [RegularExpression(@"^[a-zA-Z0-9\s]+$",
               ErrorMessage = "Brand name cannot contain special characters")]
            public string BrandName { get; set; } = null!;
            [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
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
            [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
            public string? BrandDescription { get; set; }

            public IFormFile? BrandLogo { get; set; }
        }
    }
}
