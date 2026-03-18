namespace PetCenterClient.DTOs
{
    public class ImportStockDetailDto
    {
        public Guid? ImportId { get; set; }
        public Guid ImportStockDetailId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal ImportPrice { get; set; }
        public int StockLeft { get; set; }
    }

    public class CreateImportStockDetailDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal ImportPrice { get; set; }
    }
    public class IncreaseStockItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
    public class DecreaseStockItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
