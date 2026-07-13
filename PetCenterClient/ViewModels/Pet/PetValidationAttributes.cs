using System;
using System.ComponentModel.DataAnnotations;

namespace PetCenterClient.ViewModels
{
    // Validation attribute that ensures a DateOnly value is not less than today's date.
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    // Validates that the provided date is not in the future (DateOfBirth <= today)
    public class NotLessThanTodayAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            if (value is DateOnly date)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                if (date > today)
                {
                    return new ValidationResult(ErrorMessage ?? "The date cannot be in the future.");
                }

                return ValidationResult.Success;
            }

            // If value is a nullable DateTime coming from some bindings, handle that too
            if (value is DateTime dt)
            {
                var dateOnly = DateOnly.FromDateTime(dt);
                var today = DateOnly.FromDateTime(DateTime.Today);
                if (dateOnly > today)
                {
                    return new ValidationResult(ErrorMessage ?? "The date cannot be in the future.");
                }

                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid date value");
        }
    }
}
