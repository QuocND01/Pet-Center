// File: DTOs/MedicalRecord/MedicalRecordReadDto.cs
using MedicalAPI.DTOs.Prescription;

namespace MedicalAPI.DTOs.MedicalRecord;

public class MedicalRecordReadDto
{
    public Guid RecordId { get; set; }
    public Guid AppointmentId { get; set; }
    public string Diagnosis { get; set; } = null!;
    public string Treatment { get; set; } = null!;
    public string? Note { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? Status { get; set; }
    public string StatusName => Status switch
    {
        0 => "Draft",
        1 => "InProgress",
        2 => "Completed",
        _ => "Unknown"
    };
    public List<PrescriptionItemReadDto> PrescriptionItems { get; set; } = new();
}