// File: DTOs/Prescription/PrescriptionItemCreateDto.cs
using System.ComponentModel.DataAnnotations;

namespace MedicalAPI.DTOs.Prescription;

public class PrescriptionItemCreateDto
{
    [Required(ErrorMessage = "MedicineName is required.")]
    [MaxLength(255, ErrorMessage = "MedicineName must not exceed 255 characters.")]
    public string MedicineName { get; set; } = null!;

    [Required(ErrorMessage = "Dosage is required.")]
    [MaxLength(255, ErrorMessage = "Dosage must not exceed 255 characters.")]
    public string Dosage { get; set; } = null!;

    [Required(ErrorMessage = "Duration is required.")]
    [MaxLength(255, ErrorMessage = "Duration must not exceed 255 characters.")]
    public string Duration { get; set; } = null!;

    [Required(ErrorMessage = "Quantity is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }

    [MaxLength(255, ErrorMessage = "Note must not exceed 255 characters.")]
    public string? Note { get; set; }
}