namespace PetCenterAPI.DTOs.Requests.ManageFeedback
{
    public class UpdateReplyRequestDTO
    {
        public Guid FeedbackId { get; set; }
        public string ReplyContent { get; set; } = string.Empty;
    }
}
