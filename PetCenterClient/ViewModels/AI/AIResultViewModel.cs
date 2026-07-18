using System.Text.Json.Serialization;

namespace PetCenterClient.ViewModels.AI
{
    public class AIResultViewModel
    {
        public Guid? DiseaseId { get; set; }

        public string DiseaseName { get; set; } = "";

        public double Confidence { get; set; }

        public string? Description { get; set; }

        public string? Recommendation { get; set; }

        public int? Species { get; set; }

        public bool IsDiseaseImage { get; set; }

        public bool HasDiseaseInfo { get; set; }

        public string? Message { get; set; }
    }

    public class AIDiseaseInfo
    {
        public string Diagnosis { get; set; } = null!;
        public string Treatment { get; set; } = null!;
    }

}
