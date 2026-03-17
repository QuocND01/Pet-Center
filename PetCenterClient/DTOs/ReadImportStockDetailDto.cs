

namespace PetCenterClient.DTOs
{   
    //For get by id
    public class ReadImportStockDetailDto
    {
        public Guid ImportId { get; set; }
        public Guid SupplierId { get; set; }

        public string SupplierName { get; set; } = null!;
        public Guid StaffId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? ImportDate { get; set; }
        public int Status { get; set; } 

        public List<ImportStockDetailDto> Details { get; set; } = new();
    }

    //For create
    public class CreateImportStockDto
    {
        public Guid SupplierId { get; set; }

        
        public List<CreateImportStockDetailDto> Details { get; set; } = new();
    }

    //For get all header import
    public class ReadImportHeaderDto
    {
        public Guid ImportId { get; set; }
        public Guid SupplierId { get; set; }
        public Guid StaffId { get; set; }
        public string SupplierName { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public DateTime? ImportDate { get; set; }
        public int Status { get; set; }
        
    }
    public class ImportStockResponseDto
    {
        public List<ImportDto> Imports { get; set; }
        public List<ImportDetailDto> Details { get; set; }
    }
    // All import from api for export
    public class ImportDto
    {
        public Guid ImportId { get; set; }
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; }
        public Guid StaffId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime ImportDate { get; set; }
        public int Status { get; set; }

        public List<ImportDetailDto> Details { get; set; } 
    }

    public class ImportDetailDto
    {
        public Guid ImportId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal ImportPrice { get; set; }
        public int StockLeft { get; set; }
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
