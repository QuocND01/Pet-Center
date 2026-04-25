// File: DTOs/MedicalRecord/MedicalRecordCreateDto.cs
using System.ComponentModel.DataAnnotations;
using MedicalAPI.DTOs.Prescription;

namespace MedicalAPI.DTOs.MedicalRecord;

public class MedicalRecordCreateDto
{
    [Required(ErrorMessage = "AppointmentId is required.")]
    public Guid AppointmentId { get; set; }

    [Required(ErrorMessage = "Diagnosis is required.")]
    [MaxLength(500, ErrorMessage = "Diagnosis must not exceed 500 characters.")]
    public string Diagnosis { get; set; } = null!;

    [Required(ErrorMessage = "Treatment is required.")]
    [MaxLength(500, ErrorMessage = "Treatment must not exceed 500 characters.")]
    public string Treatment { get; set; } = null!;

    [MaxLength(255, ErrorMessage = "Note must not exceed 255 characters.")]
    public string? Note { get; set; }

    public List<PrescriptionItemCreateDto>? Prescriptions { get; set; }
}