// File: DTOs/VetProfile/VetProfileReadDto.cs
namespace StaffAPI.DTOs.Staff;

public class VetProfileReadDto
{
    public Guid VetProfileId { get; set; }
    public Guid StaffId { get; set; }
    public decimal? ExperienceYears { get; set; }
    public string? Description { get; set; }
    public string? LicenseNumber { get; set; }
    public bool IsActive { get; set; }
}