using System.Text.Json.Serialization;

namespace PetCenterClient.ViewModels.AI
{
    public class AIResultViewModel
    {
        [JsonPropertyName("predicted_class")]
        public string DiseaseName { get; set; } = "";

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        public string Description { get; set; } = "";

        public string Recommendation { get; set; } = "";
    }

    public class AIDiseaseInfo
    {
        public string Diagnosis { get; set; } = null!;
        public string Treatment { get; set; } = null!;
    }

    public static class AIDiseaseData
    {
        public static readonly Dictionary<string, AIDiseaseInfo> Diseases =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["Ringworm"] = new()
                {
                    Diagnosis = "Ringworm",
                    Treatment = "Apply antifungal medication, isolate the infected pet, and disinfect the environment."
                },

                ["Dermatitis"] = new()
                {
                    Diagnosis = "Dermatitis",
                    Treatment = "Identify and eliminate the cause, administer anti-inflammatory medication, and maintain skin hygiene."
                },

                ["Tick Infection"] = new()
                {
                    Diagnosis = "Tick Infection",
                    Treatment = "Remove ticks, apply tick-control products, and monitor for tick-borne diseases."
                },

                ["Mange"] = new()
                {
                    Diagnosis = "Mange",
                    Treatment = "Administer antiparasitic medication, treat secondary infections if present, and disinfect the pet's environment."
                }
            };
    }
}
