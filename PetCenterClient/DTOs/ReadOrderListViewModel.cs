namespace PetCenterClient.DTOs
{
    public class ReadOrderListViewModel
    {
        public Guid OrderId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int Status { get; set; } // 1: Pending, 2: Processing, 3: Completed, 4: Cancelled
        public string PaymentMethod { get; set; } = null!;
        public int PaymentStatus { get; set; }
    }
}