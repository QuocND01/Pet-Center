using PetCenterAPI.Common;
using static PetCenterAPI.DTOs.Requests.PrescriptionItem.PrescriptionItemRequestDTO;
using static PetCenterAPI.DTOs.Responses.PrescriptionItem.PrescriptionItemResponseDTO;

namespace PetCenterAPI.Service.Interface
{
    public interface IPrescriptionItemService
    {
        Task<IEnumerable<ReadPrescriptionItemDTO>> GetByRecordIdAsync(Guid recordId);
        Task<ReadPrescriptionItemDTO?> GetByIdAsync(Guid id);
        Task CreateAsync(CreatePrescriptionItemDTO dto);
        Task UpdateAsync(Guid id, UpdatePrescriptionItemDTO dto);
        Task DeleteAsync(Guid id);
        Task ChangeStatusAsync(Guid id, PrescriptionItemStatus status);
    }
}
