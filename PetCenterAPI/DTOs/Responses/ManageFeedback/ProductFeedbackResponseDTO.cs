namespace PetCenterAPI.DTOs.Responses.ManageFeedback
{
    public class ProductFeedbackResponseDTO
    {
        public Guid FeedbackId { get; set; }
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public Guid ProductId { get; set; }
        public Guid OrderId { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public string? Reply { get; set; }
        public DateTime? ReplyDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<FeedbackMediaResponseDTO> MediaFiles { get; set; } = new();
    }
}
