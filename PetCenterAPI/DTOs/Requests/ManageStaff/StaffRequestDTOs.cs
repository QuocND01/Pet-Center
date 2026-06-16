using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Requests.ManageStaff
{
    /// <summary>
    /// Payload for creating a new staff member (sent as multipart/form-data
    /// because it may carry an avatar file). Vet-only fields are ignored unless
    /// the selected role is "Veterinarian".
    /// </summary>
    public class CreateStaffRequestDTO
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(50, ErrorMessage = "Full name cannot exceed 50 characters")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^(0[3|5|7|8|9])[0-9]{8}$",
            ErrorMessage = "Phone number must be a valid 10-digit Vietnamese number")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Gender is required")]
        [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Gender must be Male, Female or Other")]
        public string Gender { get; set; } = null!;

        [Required(ErrorMessage = "Birth date is required")]
        [DataType(DataType.Date)] // format: DD/MM/YYYY
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Hire date is required")]
        [DataType(DataType.Date)] // format: DD/MM/YYYY
        public DateTime HireDate { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public Guid RoleId { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = null!;

        /// <summary>Optional avatar image. Uploaded to Cloudinary when present.</summary>
        public IFormFile? Avatar { get; set; }

        // ── Vet-only fields (used only when RoleId points to the "Veterinarian" role) ──

        [StringLength(50, ErrorMessage = "License number cannot exceed 50 characters")]
        public string? LicenseNumber { get; set; }

        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        public string? Description { get; set; }

        [Range(0, 100, ErrorMessage = "Experience years must be between 0 and 100")]
        public decimal? ExperienceYears { get; set; }
    }

    /// <summary>
    /// Payload for updating an existing staff member (multipart/form-data).
    /// Rating, ExperienceYears and LicenseNumber are intentionally NOT updatable.
    /// </summary>
    public class UpdateStaffRequestDTO
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(50, ErrorMessage = "Full name cannot exceed 50 characters")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^(0[3|5|7|8|9])[0-9]{8}$",
            ErrorMessage = "Phone number must be a valid 10-digit Vietnamese number")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Gender is required")]
        [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Gender must be Male, Female or Other")]
        public string Gender { get; set; } = null!;

        [Required(ErrorMessage = "Birth date is required")]
        [DataType(DataType.Date)] // format: DD/MM/YYYY
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Hire date is required")]
        [DataType(DataType.Date)] // format: DD/MM/YYYY
        public DateTime HireDate { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public Guid RoleId { get; set; }

        /// <summary>Optional new avatar image. Replaces the existing one when present.</summary>
        public IFormFile? Avatar { get; set; }

        /// <summary>Editable vet description (only applied when staff is a Veterinarian).</summary>
        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        public string? Description { get; set; }

        /// <summary>When true, resets the staff password back to the default "123456".</summary>
        public bool ResetPassword { get; set; }
    }
}
