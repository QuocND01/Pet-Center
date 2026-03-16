namespace PetCenterClient.DTOs
{
    public class CheckoutRequestDTO
    {
        public Guid CustomerId { get; set; }
        public Guid AddressId { get; set; }
        public string? AddressSnapshot { get; set; }
        public Guid? VoucherId { get; set; }
        public List<CheckoutItemDTO> Items { get; set; } = new();
    }

    public class CheckoutItemDTO
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class CheckoutResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
    }

    public class VoucherValidateRequestDTO
    {
        public string Code { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public decimal OrderAmount { get; set; }
    }

    public class VoucherValidateResponseDTO
    {
        public bool Valid { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? VoucherId { get; set; }
        public string? Code { get; set; }
        public int? DiscountPercent { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string? Description { get; set; }
    }

    public class CustomerVoucherDTO
    {
        public Guid VoucherId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? DiscountPercent { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public bool IsUsed { get; set; }
    }

    // ViewModel for Checkout page
    public class CheckoutViewModel
    {
        public List<CheckoutCartItemVM> SelectedItems { get; set; } = new();
        public List<AddressResponseDTO> Addresses { get; set; } = new();
        public List<CustomerVoucherDTO> AvailableVouchers { get; set; } = new();
        public Guid CustomerId { get; set; }
        public decimal SubTotal => SelectedItems.Sum(i => i.SubTotal);
    }

    public class CheckoutCartItemVM
    {
        public Guid CartDetailId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public decimal SubTotal => UnitPrice * Quantity;
    }
}