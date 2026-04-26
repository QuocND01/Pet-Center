using ImportAPI.Models;
using static ImportAPI.Models.ImportStock;

namespace ImportAPI.DTOs
{   
    //For get by id
    public class ReadImportStockDto
    {
        public Guid ImportId { get; set; }
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; } = null!;
        public Guid StaffId { get; set; }
        public string InvoiceNumber { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public DateTime? ImportDate { get; set; }
        public ImportStatus Status { get; set; } 

        public List<ImportStockDetailDto> Details { get; set; } = new();
    }

    //For create
    public class CreateImportStockDto
    {
        public Guid SupplierId { get; set; }
        public Guid StaffId { get; set; }
        public string InvoiceNumber { get; set; } = null!;


        public List<CreateImportStockDetailDto> Details { get; set; } = new();
    }

    //For get all header import
    public class ReadImportHeaderDto
    {
        public Guid ImportId { get; set; }

        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; } = null!;
        public string InvoiceNumber { get; set; } = null!;
        public Guid StaffId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? ImportDate { get; set; }

        public ImportStock.ImportStatus Status { get; set; }
    }
}
