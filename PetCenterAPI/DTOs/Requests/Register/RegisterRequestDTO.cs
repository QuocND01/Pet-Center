using Microsoft.AspNetCore.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Requests.Register
{
    public class RegisterRequestDTO
    {
        private string _fullName = null!;
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(50, ErrorMessage = "Full name must not exceed 50 characters")]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]{2,}$",
            ErrorMessage = "Full name must contain letters only and at least 2 characters")]
        public string FullName
        {
            get => _fullName;
            set => _fullName = value?.Trim() ?? value!;
        }

        private string _email = null!;
        [Required(ErrorMessage = "Email is required")]
        [StringLength(50, ErrorMessage = "Email must not exceed 50 characters")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email
        {
            get => _email;
            set => _email = value?.Trim() ?? value!;
        }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^(03[2-9]|05[2689]|07[06-9]|08[1-9]|09[0-9])\d{7}$",
            ErrorMessage = "Invalid Vietnamese phone number")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [RegularExpression(@"^(?=[^a-z]*[A-Z])(?=\S+$)(?=.*[@])(?=.*[0-9]).{6,}$",
            ErrorMessage = "Password must start with uppercase, contain @, a number, and no spaces")]
        public string Password { get; set; } = null!;

        [CustomValidation(typeof(RegisterRequestDTO), nameof(ValidateBirthDay))]
        public DateOnly? BirthDay { get; set; }

        [Required(ErrorMessage = "Please select a gender")]
        public string? Gender { get; set; }

        public static ValidationResult ValidateBirthDay(DateOnly? birthDay, ValidationContext context)
        {
            if (birthDay == null) return ValidationResult.Success!;

            var today = DateOnly.FromDateTime(DateTime.Today);

            if (birthDay > today)
                return new ValidationResult("Birthday cannot be in the future");

            var age = today.Year - birthDay.Value.Year;
            if (birthDay > today.AddYears(-age)) age--;

            if (age < 16)
                return new ValidationResult("You must be at least 16 years old to register");
            if (age > 100)
                return new ValidationResult("Date of birth cannot be more than 100 years ago");

            return ValidationResult.Success!;
        }
    }
}
