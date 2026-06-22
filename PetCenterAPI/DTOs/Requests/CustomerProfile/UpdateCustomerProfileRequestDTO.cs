using System.ComponentModel.DataAnnotations;

namespace PetCenterAPI.DTOs.Requests.CustomerProfile
{
    public class UpdateCustomerProfileRequestDTO
    {
        [Required(ErrorMessage = "Full name is required")]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]{2,}$",
            ErrorMessage = "Full name must contain letters only and at least 2 characters")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression( @"^(03[2-9]|05[2689]|07[06-9]|08[1-9]|09[0-9])\d{7}$",
            ErrorMessage = "Invalid Vietnamese phone number")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Date of birth is required")]
        [CustomValidation(typeof(UpdateCustomerProfileRequestDTO), nameof(ValidateBirthday))]
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
            if (age > 100)                                          
                return new ValidationResult("Date of birth cannot be more than 100 years ago");

            return ValidationResult.Success!;
        }
    }
}
