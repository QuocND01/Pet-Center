// File: DTOs/Prescription/PrescriptionItemUpdateDto.cs
using System.ComponentModel.DataAnnotations;

namespace MedicalAPI.DTOs.Prescription;

public class PrescriptionItemUpdateDto
{
    [Required(ErrorMessage = "MedicineName is required.")]
    [MaxLength(255)]
    public string MedicineName { get; set; } = null!;

    [Required(ErrorMessage = "Dosage is required.")]
    [MaxLength(255)]
    public string Dosage { get; set; } = null!;

    [Required(ErrorMessage = "Duration is required.")]
    [MaxLength(255)]
    public string Duration { get; set; } = null!;

    [Required(ErrorMessage = "Quantity is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }

    [MaxLength(255)]
    public string? Note { get; set; }
}