namespace FeedbackAPI.DTOs
{
    public class FeedbackResponseDTO
    {
        public Guid FeedbackID { get; set; }
        public string CustomerName { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string? Reply { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
