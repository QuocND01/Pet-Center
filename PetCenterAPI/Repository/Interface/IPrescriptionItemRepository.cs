using PetCenterAPI.Common;
using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface IPrescriptionItemRepository
    {
        Task<IEnumerable<PrescriptionItem>> GetByRecordIdAsync(Guid recordId);
        Task<PrescriptionItem?> GetByIdAsync(Guid id);
        Task AddAsync(PrescriptionItem item);
        Task UpdateAsync(PrescriptionItem item);
        Task DeleteAsync(Guid id);
        Task ChangeStatusAsync(Guid id, PrescriptionItemStatus status);
    }
}
