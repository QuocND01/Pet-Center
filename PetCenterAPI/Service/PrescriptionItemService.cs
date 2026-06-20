using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;
using static PetCenterAPI.DTOs.Requests.PrescriptionItem.PrescriptionItemRequestDTO;
using static PetCenterAPI.DTOs.Responses.PrescriptionItem.PrescriptionItemResponseDTO;

namespace PetCenterAPI.Service
{
    public class PrescriptionItemService : IPrescriptionItemService
    {
        private readonly IPrescriptionItemRepository _repo;

        public PrescriptionItemService(IPrescriptionItemRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<ReadPrescriptionItemDTO>> GetByRecordIdAsync(Guid recordId)
        {
            var items = await _repo.GetByRecordIdAsync(recordId);
            return items.Select(MapToDTO);
        }

        public async Task<ReadPrescriptionItemDTO?> GetByIdAsync(Guid id)
        {
            var item = await _repo.GetByIdAsync(id);
            return item == null ? null : MapToDTO(item);
        }

        public async Task CreateAsync(CreatePrescriptionItemDTO dto)
        {
            var item = new PrescriptionItem
            {
                PrescriptionItemId = Guid.NewGuid(),
                RecordId = dto.RecordId,
                MedicineName = dto.MedicineName,
                Dosage = dto.Dosage,
                Duration = dto.Duration,
                Quantity = dto.Quantity,
                Note = dto.Note
            };

            await _repo.AddAsync(item);
        }

        public async Task UpdateAsync(Guid id, UpdatePrescriptionItemDTO dto)
        {
            var item = await _repo.GetByIdAsync(id)
                ?? throw new Exception("Prescription item not found");

            item.MedicineName = dto.MedicineName;
            item.Dosage = dto.Dosage;
            item.Duration = dto.Duration;
            item.Quantity = dto.Quantity;
            item.Note = dto.Note;

            await _repo.UpdateAsync(item);
        }

        public async Task DeleteAsync(Guid id)
        {
            var item = await _repo.GetByIdAsync(id)
                ?? throw new Exception("Prescription item not found");

            await _repo.DeleteAsync(id);
        }

        private static ReadPrescriptionItemDTO MapToDTO(PrescriptionItem p) => new()
        {
            PrescriptionItemId = p.PrescriptionItemId,
            RecordId = p.RecordId,
            MedicineName = p.MedicineName,
            Dosage = p.Dosage,
            Duration = p.Duration,
            Quantity = p.Quantity,
            Note = p.Note
        };
    }
}
