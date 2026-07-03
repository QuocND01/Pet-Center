namespace PetCenterAPI.DTOs.Requests.Import
{
    public class CreateImportDetailRequestDTO
    {

        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal ImportPrice { get; set; }
        public string SKU { get; set; } = null!;
        public string BatchCode { get; set; } = null!;
        public DateOnly? ManufacturingDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }
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
