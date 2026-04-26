// File: DTOs/VetProfile/VetProfileUpdateDto.cs
using System.ComponentModel.DataAnnotations;

namespace StaffAPI.DTOs.VetProfile;

/// <summary>
/// Chỉ cho phép cập nhật Description.
/// ExperienceYears  → cộng dồn tự động theo năm.
/// LicenseNumber    → không được update.
/// Rating           → cập nhật từ appointment service.
/// </summary>
public class VetProfileUpdateDto
{
    [MaxLength(255)]
    public string? Description { get; set; }

    public bool? IsActive { get; set; }
}