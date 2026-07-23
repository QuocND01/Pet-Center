using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Requests.ManageFeedback
{
    public class UpdateFeedbackRequestDTO
    {
        public Guid FeedbackId { get; set; }
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
        public string? Comment { get; set; }
        public List<IFormFile>? NewMediaFiles { get; set; }
        public List<string>? RemovedPublicIds { get; set; }
    }
}
