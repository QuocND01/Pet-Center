// File: DTOs/Prescription/PrescriptionItemReadDto.cs
namespace MedicalAPI.DTOs.Prescription;

public class PrescriptionItemReadDto
{
    public Guid PrescriptionItemId { get; set; }
    public Guid RecordId { get; set; }
    public string MedicineName { get; set; } = null!;
    public string Dosage { get; set; } = null!;
    public string Duration { get; set; } = null!;
    public int Quantity { get; set; }
    public string? Note { get; set; }
}