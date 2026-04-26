// File: Services/MedicalRecordService.cs
using AutoMapper;
using MedicalAPI.DTOs.MedicalRecord;
using MedicalAPI.DTOs.Prescription;
using MedicalAPI.HttpClients;
using MedicalAPI.Models;
using MedicalAPI.Repositories.Interfaces;
using MedicalAPI.Services.Interfaces;

namespace MedicalAPI.Services;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly IMedicalRecordRepository _recordRepo;
    private readonly IPrescriptionItemRepository _prescriptionRepo;
    private readonly BookingServiceClient _bookingClient;
    private readonly IMapper _mapper;

    private readonly bool _validateAppointment;

    private const int AppointmentCompletedStatus = 1;

    public MedicalRecordService(
        IMedicalRecordRepository recordRepo,
        IPrescriptionItemRepository prescriptionRepo,
        BookingServiceClient bookingClient,
        IMapper mapper,
        IConfiguration config)
    {
        _recordRepo = recordRepo;
        _prescriptionRepo = prescriptionRepo;
        _bookingClient = bookingClient;
        _mapper = mapper;
        _validateAppointment = config.GetValue<bool>("FeatureFlags:ValidateAppointmentOnCreate");
    }

    /// <inheritdoc/>
    public IQueryable<MedicalRecordReadDto> GetQueryable()
        => _mapper.ProjectTo<MedicalRecordReadDto>(_recordRepo.GetQueryable());

    /// <inheritdoc/>
    public async Task<MedicalRecordReadDto> GetByIdAsync(Guid id)
    {
        var record = await _recordRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Medical record with ID '{id}' was not found.");

        return _mapper.Map<MedicalRecordReadDto>(record);
    }

    /// <inheritdoc/>
    public async Task<List<MedicalRecordReadDto>> GetByAppointmentIdAsync(Guid appointmentId)
    {
        var records = await _recordRepo.GetByAppointmentIdAsync(appointmentId);
        return _mapper.Map<List<MedicalRecordReadDto>>(records);
    }

    /// <inheritdoc/>
    public async Task<MedicalRecordReadDto> CreateAsync(MedicalRecordCreateDto dto)
    {
        if (_validateAppointment)
        {
            var appointment = await _bookingClient.GetAppointmentAsync(dto.AppointmentId)
                ?? throw new KeyNotFoundException($"Appointment with ID '{dto.AppointmentId}' was not found in Booking Service.");

            if (appointment.Status != AppointmentCompletedStatus)
                throw new ArgumentException($"Appointment '{dto.AppointmentId}' is not completed (status must be 1). Current status: {appointment.Status}.");
        }

        var record = new MedicalRecord
        {
            RecordId = Guid.NewGuid(),
            AppointmentId = dto.AppointmentId,
            Diagnosis = dto.Diagnosis,
            Treatment = dto.Treatment,
            Note = dto.Note,
            CreatedAt = DateTime.UtcNow,
            Status = (int)MedicalRecordStatus.InProgress
        };

        var created = await _recordRepo.CreateAsync(record);

        // Create prescription items if provided
        if (dto.Prescriptions != null && dto.Prescriptions.Count > 0)
        {
            var prescriptions = dto.Prescriptions.Select(p => new PrescriptionItem
            {
                PrescriptionItemId = Guid.NewGuid(),
                RecordId = created.RecordId,
                MedicineName = p.MedicineName,
                Dosage = p.Dosage,
                Duration = p.Duration,
                Quantity = p.Quantity,
                Note = p.Note
            });

            await _prescriptionRepo.CreateRangeAsync(prescriptions);
        }

        // Reload to include prescriptions
        var result = await _recordRepo.GetByIdAsync(created.RecordId);
        return _mapper.Map<MedicalRecordReadDto>(result!);
    }

    /// <inheritdoc/>
    public async Task<MedicalRecordReadDto> UpdateAsync(Guid id, MedicalRecordUpdateDto dto)
    {
        var record = await _recordRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Medical record with ID '{id}' was not found.");

        if (record.Status == (int)MedicalRecordStatus.Completed)
            throw new ArgumentException("Cannot update a medical record that is already Completed.");

        record.Diagnosis = dto.Diagnosis;
        record.Treatment = dto.Treatment;
        record.Note = dto.Note;

        var updated = await _recordRepo.UpdateAsync(record);
        return _mapper.Map<MedicalRecordReadDto>(updated);
    }

    /// <inheritdoc/>
    public async Task<MedicalRecordReadDto> UpdateStatusAsync(Guid id, MedicalRecordStatusUpdateDto dto)
    {
        var record = await _recordRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Medical record with ID '{id}' was not found.");

        if (record.Status == (int)MedicalRecordStatus.Completed)
            throw new ArgumentException("Cannot change status of a Completed medical record.");

        record.Status = dto.Status;
        var updated = await _recordRepo.UpdateAsync(record);
        return _mapper.Map<MedicalRecordReadDto>(updated);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id)
    {
        var record = await _recordRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Medical record with ID '{id}' was not found.");

        await _recordRepo.SoftDeleteAsync(record);
    }
}