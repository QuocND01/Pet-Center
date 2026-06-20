namespace PetCenterAPI.DTOs.Requests.ManageFeedback
{
    public class BulkFeedbackFormRequestDTO
    {
        public List<FeedbackFormItemRequestDTO> Feedbacks { get; set; } = new();
    }

    public class FeedbackFormItemRequestDTO
    {
        public Guid ProductId { get; set; }
        public Guid OrderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
