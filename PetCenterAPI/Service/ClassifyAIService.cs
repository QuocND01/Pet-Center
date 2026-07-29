using PetCenterAPI.DTOs.Requests.AI;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Service
{
    public class ClassifyAIService : IClassifyAIService
    {
        private readonly IClassifyAIRepository _aiRepository;
        private readonly IDiseaseRepository _diseaseRepository;

        public ClassifyAIService(
            IClassifyAIRepository aiRepository,
            IDiseaseRepository diseaseRepository)
        {
            _aiRepository = aiRepository;
            _diseaseRepository = diseaseRepository;
        }

        public async Task<AIResultDTO?> PredictAsync(IFormFile image)
        {
            var ai = await _aiRepository.PredictAsync(image);

            if (ai == null)
                return null;

            // AI xác định ảnh không phải ảnh bệnh
            if (string.Equals(
                ai.DiseaseName,
                "not disease image",
                StringComparison.OrdinalIgnoreCase))
            {
                return new AIResultDTO
                {
                    DiseaseName = "Not_Disease",
                    Confidence = ai.Confidence,
                    IsDiseaseImage = false,
                    HasDiseaseInfo = false,
                };
            }

            // AI xác định là bệnh
            var disease = await _diseaseRepository.GetByNameAsync(ai.DiseaseName);

            // Có kết quả AI nhưng bệnh chưa có trong database
            if (disease == null)
            {
                return new AIResultDTO
                {
                    DiseaseName = ai.DiseaseName,
                    Confidence = ai.Confidence,
                    IsDiseaseImage = true,
                    HasDiseaseInfo = false,
                };
            }

            // Bệnh tồn tại trong database
            return new AIResultDTO
            {
                DiseaseId = disease.DiseaseId,
                DiseaseName = disease.Name,
                Confidence = ai.Confidence,
                Description = disease.Description,
                Recommendation = disease.Recommendation,
                Species = disease.Species,
                IsDiseaseImage = true,
                HasDiseaseInfo = true
            };
        }
    }
}
