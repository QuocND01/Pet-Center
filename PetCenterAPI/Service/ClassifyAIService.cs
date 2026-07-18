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

            if (ai.DiseaseName == "Not_Disease")
            {
                return new AIResultDTO
                {
                    DiseaseName = ai.DiseaseName,
                    Confidence = ai.Confidence,
                    IsDiseaseImage = false
                };
            }

            var disease = await _diseaseRepository.GetByNameAsync(ai.DiseaseName);

            if (disease == null)
            {
                return new AIResultDTO
                {
                    DiseaseName = ai.DiseaseName,
                    Confidence = ai.Confidence,
                    IsDiseaseImage = true,
                    HasDiseaseInfo = false
                };
            }

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
