// File: DTOs/Role/RoleReadDto.cs
namespace StaffAPI.DTOs.Staff;

public class RoleReadDto
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = null!;
}