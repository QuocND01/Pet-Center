namespace FeedbackAPI.Models
{
    public partial class FeedbackMedia
    {
        public Guid MediaId { get; set; }
        public Guid FeedbackId { get; set; }
        public string MediaUrl { get; set; } = null!;
        public string? PublicId { get; set; }
        public string MediaType { get; set; } = "image";
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }

        public virtual ProductFeedback Feedback { get; set; } = null!;
    }
}
