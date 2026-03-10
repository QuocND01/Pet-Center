using System.ComponentModel.DataAnnotations;

namespace IdentityAPI.DTOs.Resquest
{
    public class UpdateCustomerProfileRequestDto
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\u0100-\u017F\u1E00-\u1EFF]+$", ErrorMessage = "Full name can only contain letters and spaces")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^[0-9\+\-\(\)\s]{10,15}$", ErrorMessage = "Phone number must be 10-15 digits")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(UpdateCustomerProfileRequestDto), nameof(ValidateBirthday))]
        public DateOnly BirthDay { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [RegularExpression(@"^(Male|Female|Other)$", ErrorMessage = "Gender must be Male, Female, or Other")]
        public string Gender { get; set; }

        // ✅ Custom validation cho tuổi
        public static ValidationResult ValidateBirthday(DateOnly birthDay, ValidationContext context)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - birthDay.Year;

            // Kiểm tra nếu chưa qua sinh nhật năm nay
            if (birthDay > today.AddYears(-age))
                age--;

            // Tuổi phải từ 13 đến 120
            if (age < 13)
                return new ValidationResult("You must be at least 13 years old");

            if (age > 120)
                return new ValidationResult("Invalid date of birth");

            // Không được là ngày trong tương lai
            if (birthDay > today)
                return new ValidationResult("Date of birth cannot be in the future");

            return ValidationResult.Success;
        }
    }
}
