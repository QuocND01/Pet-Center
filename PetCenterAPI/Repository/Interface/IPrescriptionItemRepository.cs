using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface IPrescriptionItemRepository
    {
        Task<IEnumerable<PrescriptionItem>> GetByRecordIdAsync(Guid recordId);
        Task<PrescriptionItem?> GetByIdAsync(Guid id);
        Task<int?> GetRecordStatusAsync(Guid recordId);
        Task AddAsync(PrescriptionItem item);
        Task UpdateAsync(PrescriptionItem item);
        Task DeleteAsync(Guid id);
    }
}
