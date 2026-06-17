namespace PetCenterAPI.DTOs.Requests.ManageFeedback
{
    public class ReplyFeedbackRequestDTO
    {
        public Guid FeedbackId { get; set; }
        public Guid StaffId { get; set; }
        public string ReplyContent { get; set; } = string.Empty;
    }
}
