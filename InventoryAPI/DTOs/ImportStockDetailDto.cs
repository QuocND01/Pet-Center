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
    // Dùng cho OrderAPI gọi để trừ tồn kho khi checkout và trả lại tồn kho khi hủy đơn
    public class DeductStockRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class DeductStockResponse
    {
        public string Mapping { get; set; } = string.Empty; // "id:qty,id:qty"
    }

    public class ReturnStockRequest
    {
        public string Mapping { get; set; } = string.Empty;
    }
    public class IncreaseStockItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
