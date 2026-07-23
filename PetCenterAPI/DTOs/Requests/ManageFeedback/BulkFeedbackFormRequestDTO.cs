using System.ComponentModel.DataAnnotations;

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

        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
        public string? Comment { get; set; }
    }
}
