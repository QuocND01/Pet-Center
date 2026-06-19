namespace PetCenterAPI.DTOs.Responses.Import
{
    public class ReadImportDetailResponseDTO
    {

        public Guid? ImportId { get; set; }

        public Guid ImportStockDetailsId { get; set; }

        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public string SKU { get; set; } = null!;
        public string BatchCode { get; set; } = null!;
        public decimal ImportPrice { get; set; }
        public int StockLeft { get; set; }
        public DateTime? ManufacturingDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public ProductSnapshotRequestDTO? ImportProductSnapshot { get; set; }

    }
    public class ProductSnapshotResponseDTO{
        public Guid ImportStockDetailsId { get; set; }
        public string ProductName { get; set; } = null!;

        public string ProductCategory { get; set; } = null!;

        public string ProductBrand { get; set; } = null!;
        public string ProductImage { get; set; } = null!;
    }
    public class ProductSnapshotRequestDTO
    {
        public Guid ProductId { get; set; }

        public string ProductName { get; set; } = null!;

        public string ProductCategory { get; set; } = null!;
        public string ProductBrand { get; set; } = null!;
        public string ProductImage { get; set; } = null!;
    }
}
