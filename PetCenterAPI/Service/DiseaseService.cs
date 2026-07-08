using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;
using static PetCenterAPI.DTOs.Requests.DiseaseDTO;

namespace PetCenterAPI.Service
{
    public class DiseaseService : IDiseaseService
    {
        private readonly IDiseaseRepository _diseaseRepo;
        private readonly PetCenterContext _db; // Inject DB context cho OData Query

        public DiseaseService(IDiseaseRepository diseaseRepo, PetCenterContext db)
        {
            _diseaseRepo = diseaseRepo;
            _db = db;
        }

        // HÀM CHO ODATA (Trả về IQueryable)
        public IQueryable<ReadDiseaseDTO> GetAllDiseasesQuery()
        {
            return _db.Diseases
                .Where(d => d.IsActive == true)
                .Select(d => new ReadDiseaseDTO
                {
                    DiseaseId = d.DiseaseId,
                    Name = d.Name,
                    Description = d.Description,
                    Recommendation = d.Recommendation,
                    Species = d.Species,
                    IsSystem = d.IsSystem,
                    CreatedAt = d.CreatedAt
                });
        }

        public async Task<ReadDiseaseDTO?> GetDiseaseByIdAsync(Guid id)
        {
            var d = await _diseaseRepo.GetDiseaseByIdAsync(id);
            if (d == null) return null;

            return new ReadDiseaseDTO
            {
                DiseaseId = d.DiseaseId,
                Name = d.Name,
                Description = d.Description,
                Recommendation = d.Recommendation,
                Species = d.Species,
                IsSystem = d.IsSystem,
                CreatedAt = d.CreatedAt
            };
        }

        public async Task<bool> AddDiseaseAsync(MutateDiseaseDTO dto)
        {
            var disease = new Disease
            {
                DiseaseId = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Recommendation = dto.Recommendation,
                Species = dto.Species,
                IsActive = true,
                IsSystem = false, // User thêm vào mặc định không phải hệ thống
                CreatedAt = DateTime.UtcNow
            };

            await _diseaseRepo.AddDiseaseAsync(disease);
            await _diseaseRepo.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateDiseaseAsync(Guid id, MutateDiseaseDTO dto)
        {
            var disease = await _diseaseRepo.GetDiseaseByIdAsync(id);
            if (disease == null) return false;

            // Optional: Chặn không cho sửa tên bệnh của Hệ thống (IsSystem = true) nếu cần
            // if (disease.IsSystem) return false; 

            disease.Name = dto.Name;
            disease.Description = dto.Description;
            disease.Recommendation = dto.Recommendation;
            disease.Species = dto.Species;
            disease.UpdatedAt = DateTime.UtcNow;

            await _diseaseRepo.UpdateDiseaseAsync(disease);
            await _diseaseRepo.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteDiseaseAsync(Guid id)
        {
            var disease = await _diseaseRepo.GetDiseaseByIdAsync(id);
            if (disease == null) return false;

            // Không cho phép xóa bệnh của hệ thống
            if (disease.IsSystem) throw new Exception("Cannot delete system default diseases.");

            disease.IsActive = false; // Xóa mềm
            disease.UpdatedAt = DateTime.UtcNow;

            await _diseaseRepo.UpdateDiseaseAsync(disease);
            await _diseaseRepo.SaveAsync();
            return true;
        }
    }
}