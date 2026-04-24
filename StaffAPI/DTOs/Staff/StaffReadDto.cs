// File: DTOs/Staff/StaffReadDto.cs
namespace StaffAPI.DTOs.Staff;

public class StaffReadDto
{
    public Guid StaffId { get; set; }
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    /// <summary>format: DD/MM/YYYY</summary>
    public DateTime BirthDate { get; set; }
    public string Gender { get; set; } = null!;
    /// <summary>format: DD/MM/YYYY</summary>
    public DateTime HireDate { get; set; }
    public string Email { get; set; } = null!;
    public string? Avatar { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<RoleReadDto> Roles { get; set; } = new();
    public VetProfileReadDto? VetProfile { get; set; }
}