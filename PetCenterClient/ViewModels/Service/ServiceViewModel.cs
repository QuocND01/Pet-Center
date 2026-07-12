using PetCenterClient.Common;
using System.ComponentModel.DataAnnotations;

namespace PetCenterClient.ViewModels.Service
{
    public class ServiceViewModel
    {

        public class ReadServiceViewModel
        {
            public Guid ServiceId { get; set; }

            public string ServiceName { get; set; } = null!;

            public decimal Price { get; set; }

            public Status Status { get; set; }

            public string? ServiceDescription { get; set; }

            public int Duration { get; set; }

            public int ServiceType { get; set; }

            public List<string> ImageFiles { get; set; } = new();
        }

        public class ReadServiceViewModelForCustomer
        {
            public Guid ServiceId { get; set; }

            public string ServiceName { get; set; } = null!;

            public decimal Price { get; set; }


            public string? ServiceDescription { get; set; }

            public int Duration { get; set; }

            public int ServiceType { get; set; }

            public List<string> ImageFiles { get; set; } = new();
        }


        public class CreateServiceViewModel
        {
            [Required(ErrorMessage = "Service name is required")]
            [MaxLength(200, ErrorMessage = "Service name cannot exceed 200 characters")]
            [RegularExpression(@"^[a-zA-Z0-9\s]+$",
                ErrorMessage = "Service name cannot contain special characters")]
            public string ServiceName { get; set; } = null!;

            [Required(ErrorMessage = "Service price is required")]
            [Range(0.01, 100000000, ErrorMessage = "Service price must be greater than 0")]
            public decimal Price { get; set; }


            [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
            public string? ServiceDescription { get; set; }

            [Required(ErrorMessage = "Duration is required")]
            [Range(5, 1440, ErrorMessage = "Duration must be between 5 and 1440 minutes.")]
            public int Duration { get; set; }

            [Required(ErrorMessage = "Service type is required")]
            [Range(0, int.MaxValue)]
            public int ServiceType { get; set; }

            public List<IFormFile>? ImageFiles { get; set; } = new();
        }

        public class UpdateServiceViewModel
        {
            public Guid ServiceId { get; set; }
            [Required(ErrorMessage = "Service name is required")]
            [MaxLength(200, ErrorMessage = "Service name cannot exceed 200 characters")]
            [RegularExpression(@"^[a-zA-Z0-9\s]+$",
                 ErrorMessage = "Service name cannot contain special characters")]
            public string ServiceName { get; set; } = null!;

            [Required(ErrorMessage = "Service price is required")]
            [Range(0.01, 100000000, ErrorMessage = "Service price must be greater than 0")]
            public decimal Price { get; set; }

            [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
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
