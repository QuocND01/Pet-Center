namespace PetCenterClient.DTOs
{
    public class ApplyVoucherDTO
    {
        public Guid CustomerId { get; set; }
        public string Code { get; set; }
        public decimal OrderAmount { get; set; }
    }
}
