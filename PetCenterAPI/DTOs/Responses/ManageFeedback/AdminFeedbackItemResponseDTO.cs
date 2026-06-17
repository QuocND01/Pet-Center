namespace PetCenterAPI.DTOs.Responses.ManageFeedback
{
    public class AdminFeedbackItemResponseDTO
    {
        public Guid FeedbackId { get; set; }
        public int RowNumber { get; set; }
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImage { get; set; }
        public Guid OrderId { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public bool HasReply { get; set; }
        public string? ReplyContent { get; set; }
        public Guid? StaffId { get; set; }
        public string? StaffName { get; set; }
        public DateTime? ReplyDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? IsVisible { get; set; }
    }
}
