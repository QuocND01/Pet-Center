namespace PetCenterAPI.DTOs.Responses.ManageFeedback
{
    public class FeedbackMediaResponseDTO
    {
        public Guid MediaId { get; set; }
        public string MediaUrl { get; set; } = null!;
        public string? PublicId { get; set; }
        public string MediaType { get; set; } = "image";
    }
}
