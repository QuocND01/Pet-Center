using System.ComponentModel.DataAnnotations;

namespace CustomerAPI.DTOs.Request
{
    public class UpdateCustomerProfileRequestDto
    {
        [Required(ErrorMessage = "Full name is required")]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]{2,}$",
            ErrorMessage = "Full name must contain letters only and at least 2 characters")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^0[0-9]{9}$",
            ErrorMessage = "Phone number must start with 0 and be exactly 10 digits")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Date of birth is required")]
        [CustomValidation(typeof(UpdateCustomerProfileRequestDto), nameof(ValidateBirthday))]
        public DateOnly BirthDay { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [RegularExpression(@"^(Male|Female|Other)$",
            ErrorMessage = "Gender must be Male, Female, or Other")]
        public string Gender { get; set; } = null!;

        public static ValidationResult ValidateBirthday(DateOnly birthDay, ValidationContext context)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (birthDay > today)
                return new ValidationResult("Date of birth cannot be in the future");
            var age = today.Year - birthDay.Year;
            if (birthDay > today.AddYears(-age)) age--;
            if (age < 16)
                return new ValidationResult("You must be at least 16 years old");
            if (age > 120)
                return new ValidationResult("Invalid date of birth");
            return ValidationResult.Success!;
        }
    }
}
