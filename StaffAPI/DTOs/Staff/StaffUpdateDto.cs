// File: DTOs/Staff/StaffUpdateDto.cs
using System.ComponentModel.DataAnnotations;

namespace StaffAPI.DTOs.Staff;

public class StaffUpdateDto
{
    [MaxLength(50)]
    public string? FullName { get; set; }

    [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Invalid Vietnamese phone number.")]
    public string? PhoneNumber { get; set; }

    public DateTime? BirthDate { get; set; } // format: DD/MM/YYYY

    [RegularExpression(@"^(Male|Female|Other)$")]
    public string? Gender { get; set; }

    public DateTime? HireDate { get; set; } // format: DD/MM/YYYY

    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }

    [MinLength(8)]
    public string? Password { get; set; }

    public string? Avatar { get; set; }

    public bool? IsActive { get; set; }
}