namespace PetCenterClient.ViewModels.Inventory
{
    public class InventoryDetailViewModel
    {
        public Guid InventoryId { get; set; }

        public Guid ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Brand { get; set; } = string.Empty;

        public string? ProductImage { get; set; }

        public int QuantityAvailable { get; set; }

        public int QuantityReserved { get; set; }

        public int QuantityDamaged { get; set; }

        public DateTime LastUpdated { get; set; }

        public List<InventoryBatchViewModel> Batches { get; set; }
            = new();
    }
}
