using PetCenterAPI.Common;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;
using static PetCenterAPI.DTOs.Requests.MedicalRecord.MedicalRecordRequestDTO;
using static PetCenterAPI.DTOs.Responses.MedicalRecord.MedicalRecordResponseDTO;
using static PetCenterAPI.DTOs.Responses.PrescriptionItem.PrescriptionItemResponseDTO;

namespace PetCenterAPI.Service
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly IMedicalRecordRepository _repo;

        public MedicalRecordService(IMedicalRecordRepository repo)
        {
            _repo = repo;
        }

        public async Task<(IEnumerable<ReadMedicalRecordListDTO> Items, int Total)> GetAllAsync(
            string? search, int? status, int page, int pageSize)
        {
            var (items, total) = await _repo.GetAllAsync(search, status, page, pageSize);
            return (items.Select(MapToListDTO), total);
        }

        public async Task<IEnumerable<ReadMedicalRecordListDTO>> GetByCustomerIdAsync(Guid customerId, string? search)
        {
            var items = await _repo.GetByCustomerIdAsync(customerId, search);
            return items.Select(MapToListDTO);
        }

        public async Task<ReadMedicalRecordDTO?> GetByIdAsync(Guid id)
        {
            var record = await _repo.GetByIdAsync(id);
            if (record == null) return null;

            var snapshot = record.Appointment?.AppointmentSnapshot;

            return new ReadMedicalRecordDTO
            {
                RecordId = record.RecordId,
                AppointmentId = record.AppointmentId,
                Diagnosis = record.Diagnosis,
                Treatment = record.Treatment,
                Note = record.Note,
                CreatedAt = record.CreatedAt,
                Status = record.Status,
                StatusName = GetStatusName(record.Status),
                AppointmentStart = record.Appointment?.AppointmentStart ?? default,
                AppointmentEnd = record.Appointment?.AppointmentEnd ?? default,
                CustomerName = record.Appointment?.Customer?.FullName ?? "-",
                PetSpecies = snapshot?.Species ?? "-",
                PetBreed = snapshot?.Breed ?? "-",
                VetName = snapshot?.VetName ?? record.Appointment?.Staff?.FullName ?? "-",
                PrescriptionItems = record.PrescriptionItems.Select(p => new ReadPrescriptionItemDTO
                {
                    PrescriptionItemId = p.PrescriptionItemId,
                    RecordId = p.RecordId,
                    MedicineName = p.MedicineName,
                    Dosage = p.Dosage,
                    Duration = p.Duration,
                    Quantity = p.Quantity,
                    Note = p.Note
                }).ToList()
            };
        }

        public async Task<IEnumerable<CompletedAppointmentDTO>> GetCompletedAppointmentsAsync()
        {
            var appointments = await _repo.GetCompletedAppointmentsAsync();
            return appointments.Select(a =>
            {
                var snapshot = a.AppointmentSnapshot;
                var species = snapshot?.Species ?? a.Pet?.Species ?? "-";
                var breed = snapshot?.Breed ?? a.Pet?.Breed ?? "-";
                var vetName = snapshot?.VetName ?? "-";
                var customerName = a.Customer?.FullName ?? "-";

                return new CompletedAppointmentDTO
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentStart = a.AppointmentStart,
                    CustomerName = customerName,
                    PetSpecies = species,
                    PetBreed = breed,
                    VetName = vetName,
                    DisplayText = $"{a.AppointmentStart:dd/MM/yyyy HH:mm} | {customerName} | {species} - {breed}"
                };
            });
        }

        public async Task CreateAsync(CreateMedicalRecordDTO dto)
        {
            var record = new MedicalRecord
            {
                RecordId = Guid.NewGuid(),
                AppointmentId = dto.AppointmentId,
                Diagnosis = dto.Diagnosis,
                Treatment = dto.Treatment,
                Note = dto.Note,
                CreatedAt = DateTime.Now,
                Status = (int)MedicalRecordStatus.Drafted
            };

            await _repo.AddAsync(record);
        }

        public async Task UpdateAsync(Guid id, UpdateMedicalRecordDTO dto)
        {
            var record = await _repo.GetByIdAsync(id)
                ?? throw new Exception("Medical record not found");

            if (record.Status == (int)MedicalRecordStatus.Completed)
                throw new InvalidOperationException("Cannot update a completed medical record");

            record.Diagnosis = dto.Diagnosis;
            record.Treatment = dto.Treatment;
            record.Note = dto.Note;

            await _repo.UpdateAsync(record);
        }

        public async Task ChangeStatusAsync(Guid id, MedicalRecordStatus status)
        {
            var record = await _repo.GetByIdAsync(id)
                ?? throw new Exception("Medical record not found");

            // Completed and Cancelled are final states: no reverting backwards,
            // and a completed record can no longer be cancelled.
            if (record.Status == (int)MedicalRecordStatus.Completed)
                throw new InvalidOperationException("Cannot change the status of a completed medical record");

            if (record.Status == (int)MedicalRecordStatus.Cancelled)
                throw new InvalidOperationException("Cannot change the status of a cancelled medical record");

            await _repo.ChangeStatusAsync(id, status);
        }

        private ReadMedicalRecordListDTO MapToListDTO(MedicalRecord r)
        {
            var snapshot = r.Appointment?.AppointmentSnapshot;
            return new ReadMedicalRecordListDTO
            {
                RecordId = r.RecordId,
                AppointmentId = r.AppointmentId,
                Diagnosis = r.Diagnosis,
                Treatment = r.Treatment,
                Note = r.Note,
                CreatedAt = r.CreatedAt,
                Status = r.Status,
                StatusName = GetStatusName(r.Status),
                AppointmentStart = r.Appointment?.AppointmentStart ?? default,
                CustomerName = r.Appointment?.Customer?.FullName ?? "-",
                PetSpecies = snapshot?.Species ?? "-",
                PetBreed = snapshot?.Breed ?? "-",
                VetName = snapshot?.VetName ?? "-"
            };
        }

        private static string GetStatusName(int? status) => status switch
        {
            (int)MedicalRecordStatus.Drafted => "Drafted",
            (int)MedicalRecordStatus.Completed => "Completed",
            (int)MedicalRecordStatus.Cancelled => "Cancelled",
            _ => "Unknown"
        };
    }
}
