using System.Text.Json.Serialization;

namespace PetCenterAPI.DTOs.Requests.AI
{
    public class AIRequestDTO
    {
        [JsonPropertyName("predicted_class")]
        public string DiseaseName { get; set; } = null!;

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }

    public class AIPredictRequest
    {
        public IFormFile File { get; set; } = null!;
    }

    public class AIResultDTO
    {
        public Guid DiseaseId { get; set; }

        public string DiseaseName { get; set; } = null!;

        public double Confidence { get; set; }

        public string? Description { get; set; }

        public string? Recommendation { get; set; }

        public int Species { get; set; }
    }
}
