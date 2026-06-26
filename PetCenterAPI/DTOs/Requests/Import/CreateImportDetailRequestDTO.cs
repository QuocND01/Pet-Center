namespace PetCenterAPI.DTOs.Requests.Import
{
    public class CreateImportDetailRequestDTO
    {

        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal ImportPrice { get; set; }
        public string SKU { get; set; } = null!;
        public string BatchCode { get; set; } = null!;
        public DateOnly? ManufacturingDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }
    }

}
