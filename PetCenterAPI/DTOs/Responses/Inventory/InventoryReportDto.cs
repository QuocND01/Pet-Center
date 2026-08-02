namespace PetCenterAPI.DTOs.Responses.Inventory
{
    public class InventoryReportDto
    {
        public string SKU { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public int QuantityAvailable { get; set; }
        public int QuantityReserved { get; set; }
        public decimal AvgImportPrice { get; set; }
        public decimal TotalValuation => QuantityAvailable * AvgImportPrice;
        public DateTime LastUpdated { get; set; }
    }
    public class BatchExpiryReportDto
    {
        public string BatchCode { get; set; } = null!;
        public string InvoiceNumber { get; set; } = null!;
        public string SKU { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public decimal ImportPrice { get; set; }
        public int StockLeft { get; set; }
        public DateOnly? ManufacturingDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public string Status { get; set; } = null!;
        public string ExpiryAlert { get; set; } = null!;
    }
}
