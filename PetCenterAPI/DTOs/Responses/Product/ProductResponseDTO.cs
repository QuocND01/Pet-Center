using System.ComponentModel.DataAnnotations;
using static PetCenterAPI.DTOs.Responses.Product.ProductAttributeResponseDTO;

namespace PetCenterAPI.DTOs.Responses.Product
{
    public class ProductResponseDTO
    {

        public class UpdateProductDTO
        {
            [Required(ErrorMessage = "Product name is required")]
            [MaxLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
            [RegularExpression(@"^[a-zA-Z0-9\s]+$",
                ErrorMessage = "Product name cannot contain special characters")]
            public string ProductName { get; set; } = null!;

            [Required(ErrorMessage = "Product price is required")]
            [Range(0.01, 100000000, ErrorMessage = "Product price must be greater than 0")]
            public decimal ProductPrice { get; set; }

            [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
            public string? ProductDescription { get; set; }

            public Guid? BrandId { get; set; }

            public Guid? CategoryId { get; set; }

            public List<IFormFile>? ImageFiles { get; set; }

            public List<string>? ExistingImages { get; set; }

            public List<UpdateProductAttributeDTO>? Attributes { get; set; }
        }


        public class CreateProductDTO
        {
            [Required(ErrorMessage = "Product name is required")]
            [MaxLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
            [RegularExpression(@"^[a-zA-Z0-9\s]+$",
                ErrorMessage = "Product name cannot contain special characters")]
            public string ProductName { get; set; } = null!;

            [Required(ErrorMessage = "Product price is required")]
            [Range(0.01, 100000000, ErrorMessage = "Product price must be greater than 0")]
            public decimal ProductPrice { get; set; }

            [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
            public string? ProductDescription { get; set; }



            [Required(ErrorMessage = "Brand is required")]
            public Guid BrandId { get; set; }

            [Required(ErrorMessage = "Category is required")]
            public Guid CategoryId { get; set; }

            public List<IFormFile>? ImageFiles { get; set; }

            public List<CreateProductAttributeDTO>? Attributes { get; set; }
        }
    }
}
