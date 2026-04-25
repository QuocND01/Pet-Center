// File: DTOs/MedicalRecord/MedicalRecordStatusUpdateDto.cs
using System.ComponentModel.DataAnnotations;

namespace MedicalAPI.DTOs.MedicalRecord;

public class MedicalRecordStatusUpdateDto
{
    [Required(ErrorMessage = "Status is required.")]
    [Range(0, 2, ErrorMessage = "Status must be 0 (Draft), 1 (InProgress), or 2 (Completed).")]
    public int Status { get; set; }
}