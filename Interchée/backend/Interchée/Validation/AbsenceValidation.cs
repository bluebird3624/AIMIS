using Interchée.Dtos;
using System.ComponentModel.DataAnnotations;

namespace Interchée.Validation
{
    public static class AbsenceValidation
    {
        public static ValidationResult? ValidateDates(DateTime endDate, ValidationContext context)
        {
            if (context.ObjectInstance is CreateAbsenceRequestDto instance && endDate < instance.StartDate)
            {
                return new ValidationResult("End date cannot be before start date");
            }
            return ValidationResult.Success;
        }
    }

    public static class InternValidation
    {
        public static ValidationResult? ValidateInternshipDates(DateTime endDate, ValidationContext context)
        {
            var instance = context.ObjectInstance as CreateInternDto;
            if (instance != null && endDate < instance.StartDate)
            {
                return new ValidationResult("Internship end date cannot be before start date");
            }
            if (instance != null && endDate < DateTime.Today)
            {
                return new ValidationResult("Internship end date cannot be in the past");
            }
            return ValidationResult.Success;
        }
    }
}