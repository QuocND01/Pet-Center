namespace InventoryAPI.DTOs
{
    public class ImportStockDetailDto
    {
        public Guid? ImportId { get; set; } 
        public Guid ImportStockDetailId { get; set; }
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
    public class ImportExportResponseDto
    {
        public List<ReadImportHeaderDto> Imports { get; set; } = new();
        public List<ImportStockDetailDto> Details { get; set; } = new();
    }
}
