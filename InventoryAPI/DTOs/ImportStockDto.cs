using static InventoryAPI.Models.ImportStock;

namespace InventoryAPI.DTOs
{
    public class ImportStockDto
    {
        public Guid ImportId { get; set; }
        public Guid SupplierId { get; set; }
        public Guid StaffId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? ImportDate { get; set; }
        public ImportStatus Status { get; set; } 

        public List<ImportStockDetailDto> Details { get; set; } = new();
    }

    public class CreateImportStockDto
    {
        public Guid SupplierId { get; set; }
        public Guid StaffId { get; set; }
        

        public List<CreateImportStockDetailDto> Details { get; set; } = new();
    }
}
