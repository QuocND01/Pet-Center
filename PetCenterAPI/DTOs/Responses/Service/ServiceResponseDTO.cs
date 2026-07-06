using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Responses.Service
{
    public class ServiceResponseDTO
    {

        public class CreateServiceDTO
        {
            [Required(ErrorMessage = "Service name is required")]
            [MaxLength(200, ErrorMessage = "Service name cannot exceed 200 characters")]
            [RegularExpression(@"^[a-zA-Z0-9\s]+$",
                ErrorMessage = "Service name cannot contain special characters")]
            public string ServiceName { get; set; } = null!;

            [Required(ErrorMessage = "Service price is required")]
            [Range(0.01, 100000000, ErrorMessage = "Service price must be greater than 0")]
            public decimal Price { get; set; }


            [MaxLength(20000, ErrorMessage = "Description cannot exceed 20000 characters")]
            public string? ServiceDescription { get; set; }

            [Required(ErrorMessage = "Duration is required")]
            [Range(5, 1440, ErrorMessage = "Duration must be between 5 and 1440 minutes.")]
            public int Duration { get; set; }

            [Required(ErrorMessage = "Service type is required")]
            [Range(0, int.MaxValue)]
            public int ServiceType { get; set; }

            public List<IFormFile>? ImageFiles { get; set; } = new();
        }

        public class UpdateServiceDTO
        {
            [Required(ErrorMessage = "Service name is required")]
            [MaxLength(200, ErrorMessage = "Service name cannot exceed 200 characters")]
            [RegularExpression(@"^[a-zA-Z0-9\s]+$",
                ErrorMessage = "Service name cannot contain special characters")]
            public string ServiceName { get; set; } = null!;

            [Required(ErrorMessage = "Service price is required")]
            [Range(0.01, 100000000, ErrorMessage = "Service price must be greater than 0")]
            public decimal Price { get; set; }

            [MaxLength(20000, ErrorMessage = "Description cannot exceed 20000 characters")]
            public string? ServiceDescription { get; set; }

            [Required(ErrorMessage = "Duration is required")]
            [Range(5, 1440, ErrorMessage = "Duration must be between 5 and 1440 minutes.")]
            public int Duration { get; set; }

            [Required(ErrorMessage = "Service type is required")]
            [Range(0, int.MaxValue)]
            public int ServiceType { get; set; }

            public List<string>? ExistingImages { get; set; } = new();

            public List<IFormFile>? ImageFiles { get; set; } = new();
        }
    }
}
