

namespace PetCenterClient.ViewModels
{   
    //For get by id
    public class ImportStockViewModel
    {
        public Guid ImportId { get; set; }
        public Guid SupplierId { get; set; }

        public string SupplierName { get; set; } = null!;
        public Guid StaffId { get; set; }
        public string StaffName { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public DateTime? ImportDate { get; set; }
        public int Status { get; set; }
        public string InvoiceNumber { get; set; } = null!;
        public string? Note { get; set; }

        public List<ImportDetailViewModel> Details { get; set; } = new();
    }

    //For create
    public class CreateImportViewModel
    {
        public Guid SupplierId { get; set; }
        public string InvoiceNumber { get; set; } = null!;
        public string? Note { get; set; }

        public List<CreateImportDetailViewModel> Details { get; set; } = new();

    }

    //For get all header import
    public class ImportHeaderViewModel
    {
        public Guid ImportId { get; set; }
        public Guid SupplierId { get; set; }
        public Guid StaffId { get; set; }
        public string StaffName { get; set; } = null!;
        public string SupplierName { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public DateTime? ImportDate { get; set; }
        public int Status { get; set; }
        
    }
    public class ImportStockResponseDto
    {
        public List<ImportStockViewModel> Imports { get; set; } = null!;
        public List<ImportDetailViewModel> Details { get; set; } = null!;
    }

    //Export
    public class ImportStockExcelDto
    {
        public string Code { get; set; }

        public string SupplierName { get; set; }

        public string StaffName { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime ImportDate { get; set; }

        public string Status { get; set; }

        public List<ImportStockDetailExcelDto> Details { get; set; } = new();
    }
    public class ImportStockDetailExcelDto
    {
        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public decimal ImportPrice { get; set; }

        public int StockLeft { get; set; }
    }
}
