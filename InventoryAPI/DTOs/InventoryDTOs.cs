namespace InventoryAPI.DTOs
{
    public class ProductInventoryDTO
    {
        public Guid ProductId { get; set; }
        public int QuantityAvailable { get; set; }
    }
}
