namespace FeedbackAPI.DTOs
{
    public class CreateFeedbackDTO
    {
        public Guid CustomerId { get; set; }
        public Guid ProductId { get; set; }
        public Guid OrderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }

    }
}
