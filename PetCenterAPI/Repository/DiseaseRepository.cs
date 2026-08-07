using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;

namespace PetCenterAPI.Repository
{
    public class DiseaseRepository : IDiseaseRepository
    {
        private readonly PetCenterContext _db;
        public DiseaseRepository(PetCenterContext db) => _db = db;

        public async Task<Disease?> GetDiseaseByIdAsync(Guid id)
        {
            return await _db.Diseases
                .FirstOrDefaultAsync(d => d.DiseaseId == id && d.IsActive == true);
        }

        public async Task AddDiseaseAsync(Disease disease)
        {
            await _db.Diseases.AddAsync(disease);
        }

        public Task UpdateDiseaseAsync(Disease disease)
        {
            _db.Diseases.Update(disease);
            return Task.CompletedTask;
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task<Disease?> GetByNameAsync(string name)
        {
            // Tìm theo tên không phân biệt hoa thường và đang hoạt động
            var lowered = name?.Trim().ToLower();
            return await _db.Diseases
                .Where(x => x.IsActive == true && x.Name != null)
                .FirstOrDefaultAsync(x => x.Name.ToLower() == lowered);
        }
    }
}