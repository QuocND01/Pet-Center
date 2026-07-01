namespace PetCenterAPI.DTOs.Responses.Inventory
{
    public class InventoryItemResponseDTO
    {
        public Guid InventoryId { get; set; }

        public Guid ProductId { get; set; }

        public string ProductName { get; set; } = null!;

        public string SKU { get; set; } = null!;

        public string Category { get; set; } = null!;

        public string Brand { get; set; } = null!;

        public string? ProductImage { get; set; }

        public int QuantityAvailable { get; set; }

        public int QuantityReserved { get; set; }

        public int QuantityDamaged { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
