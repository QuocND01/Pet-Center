namespace PetCenterAPI.DTOs.Responses.ChatBot
{
    public class ChatbotPendingOrderResponseDTO
    {
        // ============================================================
        // PENDING ORDERS RESPONSE
        // ============================================================
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public int StatusCode { get; set; }
        public string PaymentMethod { get; set; } = null!;
    }

    public class ChatbotLatestOrderResponseDTO
    {
        // ============================================================
        // LATEST ORDER STATUS RESPONSE
        // ============================================================
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public int StatusCode { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public DateTime? DeliveredDate { get; set; }
    }
}
