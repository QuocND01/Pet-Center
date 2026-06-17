namespace PetCenterClient.DTOs
{
    public class ReadOrderDetailViewModel
    {
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public int Status { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public int PaymentStatus { get; set; }
        public string AddressSnapshot { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public List<ReadOrderItemViewModel> OrderItems { get; set; } = new();
    }

    public class ReadOrderItemViewModel
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string ProductCategory { get; set; } = null!;
        public string ProductBrand { get; set; } = null!;
        public string ProductImage { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal => Quantity * UnitPrice;
    }
}