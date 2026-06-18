namespace PetCenterAPI.DTOs.Requests.Import
{
    public class CreateImportRequestDTO
    {
        public Guid SupplierId { get; set; }
        public Guid StaffId { get; set; }
        public string InvoiceNumber { get; set; } = null!;
        public string? Note { get; set; }
        public List<CreateImportDetailRequestDTO> Details { get; set; } = new();
    }
}
