// File: DTOs/Staff/StaffCreateDto.cs
using System.ComponentModel.DataAnnotations;
using StaffAPI.DTOs.VetProfile;

namespace StaffAPI.DTOs.Staff;

public class StaffCreateDto
{
    [Required]
    [MaxLength(50)]
    public string FullName { get; set; } = null!;

    [Required]
    [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Invalid Vietnamese phone number.")]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    public DateTime BirthDate { get; set; } // format: DD/MM/YYYY

    [Required]
    [RegularExpression(@"^(Male|Female|Other)$", ErrorMessage = "Gender must be Male, Female, or Other.")]
    public string Gender { get; set; } = null!;

    [Required]
    public DateTime HireDate { get; set; } // format: DD/MM/YYYY

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = null!;

    public string? Avatar { get; set; }

    /// <summary>RoleIds to assign. At least one required.</summary>
    [Required]
    [MinLength(1)]
    public List<Guid> RoleIds { get; set; } = new();

    /// <summary>Required only when role is Vet.</summary>
    public VetProfileCreateDto? VetProfile { get; set; }
}