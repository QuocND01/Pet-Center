// File: DTOs/Role/RoleReadDto.cs
namespace StaffAPI.DTOs.Role;

public class RoleReadDto
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = null!;
}