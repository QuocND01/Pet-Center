using ProductAPI.Models;

namespace ProductAPI.DTOs
{
    public class ReadProductDTO
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal ProductPrice { get; set; }
        public string? ProductDescription { get; set; }
        public int? StockQuantity { get; set; }

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
        public string ProductName { get; set; } = null!;
        public decimal ProductPrice { get; set; }
        public string? ProductDescription { get; set; }
        public int? StockQuantity { get; set; }

        public Guid? BrandId { get; set; }
        public Guid? CategoryId { get; set; }

        public List<IFormFile>? ImageFiles { get; set; }
        public List<string>? ExistingImages { get; set; }

        public List<UpdateProductAttributeDTO>? Attributes { get; set; }
    }

    public class CreateProductDTO
    {
        public string ProductName { get; set; } = null!;
        public decimal ProductPrice { get; set; }
        public string? ProductDescription { get; set; }
        public int? StockQuantity { get; set; }

        public Guid BrandId { get; set; }
        public Guid CategoryId { get; set; }

        public List<IFormFile>? ImageFiles { get; set; }

        public List<CreateProductAttributeDTO>? Attributes { get; set; }
    }
    public class SelectProductDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = null!;
    }
    }
