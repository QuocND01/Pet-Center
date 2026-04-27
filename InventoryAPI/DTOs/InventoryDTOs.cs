namespace InventoryAPI.DTOs
{
    public class ProductQuantityDTO
    {
        public Guid ProductId { get; set; }
        public int QuantityAvailable { get; set; }
    }
    public class ReadInventoryDto
    {
        public Guid InventoryId { get; set; }
        public Guid? ProductId { get; set; }
        public int QuantityAvailable { get; set; }
        public DateTime LastUpdated { get; set; }
    }
    public class ReadTransactionDto
    {
        public Guid TransactionId { get; set; }
        public Guid InventoryId { get; set; }
        public int QuantityChange { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid ReferenceId { get; set; }
        public string ReferenceType { get; set; } = null!;
        public string? Note { get; set; }
    }
}
