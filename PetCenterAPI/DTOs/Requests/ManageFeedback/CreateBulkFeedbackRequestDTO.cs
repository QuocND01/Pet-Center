namespace PetCenterAPI.DTOs.Requests.ManageFeedback
{
    public class CreateBulkFeedbackRequestDTO
    {
        public List<CreateFeedbackItemRequestDTO> Feedbacks { get; set; } = new();
    }

    public class CreateFeedbackItemRequestDTO
    {
        public Guid ProductId { get; set; }
        public Guid OrderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public List<IFormFile>? MediaFiles { get; set; }
    }
}
