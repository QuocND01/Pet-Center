using PetCenterAPI.Models;

namespace PetCenterAPI.DTOs.Responses.Import

{
    public class ReadImportResponseDTO
    {
        public Guid ImportId { get; set; }

        public Guid SupplierId { get; set; }

        public string SupplierName { get; set; } = null!;

        public Guid StaffId { get; set; }

        public string StaffName { get; set; } = null!;
        public string InvoiceNumber { get; set; } = null!;

        public decimal TotalAmount { get; set; }

        public DateTime? ImportDate { get; set; }
        public string? Note { get; set; }
        public ImportStock.ImportStatus Status { get; set; }

        public List<ReadImportDetailResponseDTO> Details { get; set; }
            = new();
    }

    public class ReadImportHeaderResponseDTO
    {
        public Guid ImportId { get; set; }

        public Guid SupplierId { get; set; }
        public string StaffName { get; set; } = null!;

        public string SupplierName { get; set; } = null!;

        public string InvoiceNumber { get; set; } = null!;

        public Guid StaffId { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime? ImportDate { get; set; }

        public ImportStock.ImportStatus Status { get; set; }
    }
    public class ExportResponseDTO
    {
        public List<ReadImportHeaderResponseDTO> Imports { get; set; } = new();
        public List<ReadImportDetailResponseDTO> Details { get; set; } = new();
    }
}
