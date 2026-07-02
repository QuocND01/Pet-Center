namespace PetCenterClient.ViewModels.Inventory
{
    public class InventoryBatchViewModel
    {
        public Guid ImportStockDetailsId { get; set; }

        public string SKU { get; set; } = string.Empty;

        public string BatchCode { get; set; } = string.Empty;

        public decimal ImportPrice { get; set; }

        public int Quantity { get; set; }

        public int StockLeft { get; set; }

        public int QuantitySold { get; set; }

        public BatchStatus BatchStatus { get; set; } = BatchStatus.Active;

        public DateOnly? ManufacturingDate { get; set; }

        public DateOnly? ExpiryDate { get; set; }

        public DateTime CreatedAt { get; set; }
    }
    public enum BatchStatus
    {
        Active,
        Exhausted,
        Expired,
        Quarantine
    }
}
