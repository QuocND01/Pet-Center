using PetCenterAPI.Models;

namespace PetCenterAPI.DTOs.Responses.Inventory
{
    public class InventoryDetailResponseDTO
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

        public List<InventoryBatchResponseDTO> Batches { get; set; }
            = new();
        public List<InventoryTransactionResponseDTO> Transactions { get; set; }
            = new();
        
    }
    public class InventoryBatchResponseDTO
    {
        public Guid ImportStockDetailsId { get; set; }

        public string SKU { get; set; } = null!;

        public string BatchCode { get; set; } = null!;

        public decimal ImportPrice { get; set; }

        public int Quantity { get; set; }

        public int StockLeft { get; set; }

        public int QuantitySold { get; set; }

        public BatchStatus BatchStatus { get; set; }

        public DateOnly? ManufacturingDate { get; set; }

        public DateOnly? ExpiryDate { get; set; }

        public DateTime CreatedAt { get; set; }
    }
    public class InventoryTransactionResponseDTO
    {
        public Guid TransactionId { get; set; }
        public int QuantityChange { get; set; }

        // 🆕 snapshot trước/sau để reconstruct lịch sử
        public int QuantityBefore { get; set; }
        public int QuantityAfter { get; set; }
        public string? TransactionType { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
