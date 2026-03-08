namespace PetCenterClient.DTOs
{
    public class LowStockDto
    {
        public Guid ProductId { get; set; }
        public int StockLeft { get; set; }
    }

    public class TopImportedProductDto
    {
        public Guid ProductId { get; set; }
        public int TotalImported { get; set; }
    }

    public class ImportByMonthDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalQuantity { get; set; }
    }
}
