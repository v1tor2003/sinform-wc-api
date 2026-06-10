using System;
using System.ComponentModel.DataAnnotations;

namespace SinformWcApi.Validation;

public class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime dateTime)
        {
            if (dateTime.ToUniversalTime() <= DateTime.UtcNow)
            {
                return new ValidationResult(ErrorMessage ?? "Guesses deadline must be in the future.");
            }
            return ValidationResult.Success;
        }
        return new ValidationResult("Invalid date value.");
    }
}
