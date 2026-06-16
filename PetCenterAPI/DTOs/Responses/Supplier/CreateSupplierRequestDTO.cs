namespace PetCenterAPI.DTOs.Responses.Supplier
{
    public class CreateSupplierRequestDTO
    {
        public string? TaxId { get; set; }
        public string SupplierName { get; set; } = null!;
        public string SupplierEmail { get; set; } = null!;
        public string SupplierPhoneNumber { get; set; } = null!;
        public string SupplierAddress { get; set; } = null!;
        public string? ContactPerson { get; set; }
        public string? SupplierDescription { get; set; }
    }
}
