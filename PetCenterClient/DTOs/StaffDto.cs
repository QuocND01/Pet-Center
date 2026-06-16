using System.ComponentModel.DataAnnotations;

namespace PetCenterClient.DTOs
{
    // ============================================================
    // READ DTOs (responses from the API)
    // ============================================================

    /// <summary>Row shown in the staff management list.</summary>
    public class StaffListItemDto
    {
        public Guid StaffId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime HireDate { get; set; }
        public string? Avatar { get; set; }
        public bool IsActive { get; set; }
        public string? RoleName { get; set; }
        public Guid? RoleId { get; set; }
    }

    public class StaffDetailDto
    {
        public Guid StaffId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime HireDate { get; set; }
        public string? Avatar { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? RoleName { get; set; }
        public Guid? RoleId { get; set; }
        public VetProfileDto? VetProfile { get; set; }
    }

    public class VetProfileDto
    {
        public Guid VetProfileId { get; set; }
        public decimal? ExperienceYears { get; set; }
        public string? Description { get; set; }
        public string? LicenseNumber { get; set; }
        public decimal Rating { get; set; }
    }

    public class RoleDto
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = null!;
    }

    // ============================================================
    // WRITE DTOs (form payloads)
    // ============================================================

    public class CreateStaffDto
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(50, ErrorMessage = "Full name cannot exceed 50 characters")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^(0[3|5|7|8|9])[0-9]{8}$",
            ErrorMessage = "Phone number must be a valid 10-digit Vietnamese number")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = null!;

        [Required(ErrorMessage = "Birth date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? BirthDate { get; set; }

        [Required(ErrorMessage = "Hire date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? HireDate { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public Guid RoleId { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Please confirm the password")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = null!;

        public IFormFile? Avatar { get; set; }

        // Vet-only fields (validated server-side when role is Veterinarian)
        public string? LicenseNumber { get; set; }
        public string? Description { get; set; }
        public decimal? ExperienceYears { get; set; }
    }

    public class UpdateStaffDto
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(50, ErrorMessage = "Full name cannot exceed 50 characters")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^(0[3|5|7|8|9])[0-9]{8}$",
            ErrorMessage = "Phone number must be a valid 10-digit Vietnamese number")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = null!;

        [Required(ErrorMessage = "Birth date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? BirthDate { get; set; }

        [Required(ErrorMessage = "Hire date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? HireDate { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public Guid RoleId { get; set; }

        public IFormFile? Avatar { get; set; }

        /// <summary>Editable vet description (only applied when staff is a Veterinarian).</summary>
        public string? Description { get; set; }

        /// <summary>When true, resets the password back to the default "123456".</summary>
        public bool ResetPassword { get; set; }
    }

    /// <summary>Used by other modules (e.g. import stocks) to populate staff name dropdowns.</summary>
    public class StaffNameListDto
    {
        public Guid StaffId { get; set; }
        public string StaffName { get; set; } = null!;
    }
}
