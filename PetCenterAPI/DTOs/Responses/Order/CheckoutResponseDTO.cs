namespace PetCenterAPI.DTOs.Responses.Order
{
    public class PlaceOrderResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
    }

    public class AvailableVoucherDTO
    {
        public Guid VoucherId { get; set; }
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public int? DiscountPercent { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public DateTime? ExpiredDate { get; set; }
    }
}
