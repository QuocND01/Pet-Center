namespace PetCenterClient.ViewModels.Inventory
{
    public class InventoryTransactionViewModel
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
