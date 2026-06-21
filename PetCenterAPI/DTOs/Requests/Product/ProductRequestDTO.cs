using PetCenterAPI.Common;
using static PetCenterAPI.DTOs.Requests.Product.ProductAttributeRequestDTO;

namespace PetCenterAPI.DTOs.Requests.Product
{
    public class ProductRequestDTO
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
            public Status Status { get; set; }

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
            public List<ProductAttributeDTO>? Attributes { get; set; }

            // SKU-vinh
            public string SKU { get; set; } = null!;
        }

        public class ReadProductDTOForCustomer
        {
            public Guid ProductId { get; set; }
            public string ProductName { get; set; } = null!;
            public decimal ProductPrice { get; set; }
            public string? ProductDescription { get; set; }
            public int StockQuantity { get; set; } = 0;
            public DateTime? AddedAt { get; set; }

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
            public List<ProductAttributeDTO>? Attributes { get; set; }
        }

        public class SelectProductDTO
        {
            public Guid ProductId { get; set; }
            public string ProductName { get; set; } = null!;
        }
        public class IncreaseStockItemDTO
        {
            public Guid ProductId { get; set; }
            public int Quantity { get; set; }
        }
        public class DecreaseStockItemDTO
        {
            public Guid ProductId { get; set; }
            public int Quantity { get; set; }
        }

        public class StockDTO
        {
            public Guid ProductId { get; set; }
            public int QuantityAvailable { get; set; }
        }
    }
}
