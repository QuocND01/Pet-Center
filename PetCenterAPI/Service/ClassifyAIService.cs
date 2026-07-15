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
            Console.WriteLine(ai.DiseaseName);
            Console.WriteLine(ai.Confidence);
            if (ai == null)
                return null;

            var disease = await _diseaseRepository.GetByNameAsync(ai.DiseaseName);

            if (disease == null)
                return null;

            return new AIResultDTO
            {
                DiseaseId = disease.DiseaseId,
                DiseaseName = disease.Name,
                Confidence = ai.Confidence,
                Description = disease.Description,
                Recommendation = disease.Recommendation,
                Species = disease.Species
            };
        }
    }
}
