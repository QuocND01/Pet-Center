// File: Services/PrescriptionItemService.cs
using AutoMapper;
using MedicalAPI.DTOs.Prescription;
using MedicalAPI.Models;
using MedicalAPI.Repositories.Interfaces;
using MedicalAPI.Services.Interfaces;

namespace MedicalAPI.Services;

public class PrescriptionItemService : IPrescriptionItemService
{
    private readonly IPrescriptionItemRepository _prescriptionRepo;
    private readonly IMedicalRecordRepository _recordRepo;
    private readonly IMapper _mapper;

    public PrescriptionItemService(
        IPrescriptionItemRepository prescriptionRepo,
        IMedicalRecordRepository recordRepo,
        IMapper mapper)
    {
        _prescriptionRepo = prescriptionRepo;
        _recordRepo = recordRepo;
        _mapper = mapper;
    }

    /// <summary>Validates parent record exists and is not Completed.</summary>
    private async Task<Models.MedicalRecord> GetEditableRecordAsync(Guid recordId)
    {
        var record = await _recordRepo.GetByIdAsync(recordId)
            ?? throw new KeyNotFoundException($"Medical record with ID '{recordId}' was not found.");

        if (record.Status == (int)MedicalRecordStatus.Completed)
            throw new ArgumentException("Cannot modify prescriptions for a Completed medical record.");

        return record;
    }

    /// <inheritdoc/>
    public async Task<List<PrescriptionItemReadDto>> GetByRecordIdAsync(Guid recordId)
    {
        // Ensure record exists
        _ = await _recordRepo.GetByIdAsync(recordId)
            ?? throw new KeyNotFoundException($"Medical record with ID '{recordId}' was not found.");

        var items = await _prescriptionRepo.GetByRecordIdAsync(recordId);
        return _mapper.Map<List<PrescriptionItemReadDto>>(items);
    }

    /// <inheritdoc/>
    public async Task<PrescriptionItemReadDto> GetByIdAsync(Guid id)
    {
        var item = await _prescriptionRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Prescription item with ID '{id}' was not found.");

        return _mapper.Map<PrescriptionItemReadDto>(item);
    }

    /// <inheritdoc/>
    public async Task<PrescriptionItemReadDto> CreateAsync(Guid recordId, PrescriptionItemCreateDto dto)
    {
        await GetEditableRecordAsync(recordId);

        var item = new PrescriptionItem
        {
            PrescriptionItemId = Guid.NewGuid(),
            RecordId = recordId,
            MedicineName = dto.MedicineName,
            Dosage = dto.Dosage,
            Duration = dto.Duration,
            Quantity = dto.Quantity,
            Note = dto.Note
        };

        var created = await _prescriptionRepo.CreateAsync(item);
        return _mapper.Map<PrescriptionItemReadDto>(created);
    }

    /// <inheritdoc/>
    public async Task<PrescriptionItemReadDto> UpdateAsync(Guid id, PrescriptionItemUpdateDto dto)
    {
        var item = await _prescriptionRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Prescription item with ID '{id}' was not found.");

        await GetEditableRecordAsync(item.RecordId);

        item.MedicineName = dto.MedicineName;
        item.Dosage = dto.Dosage;
        item.Duration = dto.Duration;
        item.Quantity = dto.Quantity;
        item.Note = dto.Note;

        var updated = await _prescriptionRepo.UpdateAsync(item);
        return _mapper.Map<PrescriptionItemReadDto>(updated);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id)
    {
        var item = await _prescriptionRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Prescription item with ID '{id}' was not found.");

        await GetEditableRecordAsync(item.RecordId);
        await _prescriptionRepo.DeleteAsync(item);
    }
}