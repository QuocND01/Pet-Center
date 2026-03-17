using System.ComponentModel.DataAnnotations;

namespace IdentityAPI.DTOs.Resquest
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Full name is required")]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]{2,}$",
            ErrorMessage = "Full name must contain letters only and at least 2 characters")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^0[0-9]{9}$",
            ErrorMessage = "Phone number must start with 0 and be exactly 10 digits")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [RegularExpression(@"^(?=[^a-z]*[A-Z])(?=\S+$)(?=.*[@])(?=.*[0-9]).{6,}$",
            ErrorMessage = "Password must start with uppercase, contain @, a number, and no spaces")]
        public string Password { get; set; } = null!;

        [CustomValidation(typeof(RegisterDto), nameof(ValidateBirthDay))]
        public DateOnly? BirthDay { get; set; }

        [Required(ErrorMessage = "Please select a gender")]
        public string? Gender { get; set; }

        public static ValidationResult ValidateBirthDay(DateOnly? birthDay, ValidationContext context)
        {
            if (birthDay == null) return ValidationResult.Success;

            var today = DateOnly.FromDateTime(DateTime.Today);
            if (birthDay > today)
                return new ValidationResult("Birthday cannot be in the future");

            var minAge = today.AddYears(-16);
            if (birthDay > minAge)
                return new ValidationResult("You must be at least 16 years old to register");

            return ValidationResult.Success;
        }
    }

    public class VerifyOtpDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        public string Code { get; set; } = null!;
    }

    public class ResendOtpDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
