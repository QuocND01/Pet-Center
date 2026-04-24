// File: DTOs/VetProfile/VetProfileCreateDto.cs
using System.ComponentModel.DataAnnotations;

namespace StaffAPI.DTOs.Staff;

public class VetProfileCreateDto
{
    [MaxLength(255)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? LicenseNumber { get; set; }
}