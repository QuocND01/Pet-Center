namespace InventoryAPI.DTOs
{
    public class ReadSupplierDto
    {
        public Guid SupplierId { get; set; }
        public string? TaxId { get; set; }
        public string SupplierName { get; set; } = null!;
        public string SupplierEmail { get; set; } = null!;
        public string SupplierPhoneNumber { get; set; } = null!;
        public string SupplierAddress { get; set; } = null!;
        public string? ContactPersion { get; set; }
        public string? SupplierDescription { get; set; }
        public bool IsActive { get; set; }
    }
    public class CreateSupplierDto
    {
        public string? TaxId { get; set; }
        public string SupplierName { get; set; } = null!;
        public string SupplierEmail { get; set; } = null!;
        public string SupplierPhoneNumber { get; set; } = null!;
        public string SupplierAddress { get; set; } = null!;
        public string? ContactPersion { get; set; }
        public string? SupplierDescription { get; set; }
    }
    public class UpdateSupplierDto
    {
        public string? TaxId { get; set; }
        public string SupplierName { get; set; } = null!;
        public string SupplierEmail { get; set; } = null!;
        public string SupplierPhoneNumber { get; set; } = null!;
        public string SupplierAddress { get; set; } = null!;
        public string? ContactPersion { get; set; }
        public string? SupplierDescription { get; set; }
    }
}
