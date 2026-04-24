using ProductAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace ProductAPI.DTOs
{
    public class ReadProductDTO
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal ProductPrice { get; set; }
        public string? ProductDescription { get; set; }
        public int StockQuantity { get; set; } = 0;

        public DateTime? AddedAt { get; set; }
        public DateTime? UpdateAt { get; set; }

        // Brand
        public Guid BrandId { get; set; }
        public string? BrandName { get; set; }
        public string? BrandLogo { get; set; }

        // Category
        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }


        // Images
        public List<string>? Images { get; set; }

        // Attributes
        public List<ProductAttributedto>? Attributes { get; set; }
    }

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


    public class SelectProductDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = null!;
    }
    public class IncreaseStockItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
    public class DecreaseStockItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
