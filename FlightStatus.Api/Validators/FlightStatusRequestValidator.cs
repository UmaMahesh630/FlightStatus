using FluentValidation;
using FlightStatus.Api.Dtos;

namespace FlightStatus.Api.Validators;

/// <summary>
/// Defines FluentValidation rules for <see cref="FlightStatusRequest"/>.
/// </summary>
/// <remarks>
/// Encapsulates format validation (Regex checks) and logical calendar date validation.
/// </remarks>
public class FlightStatusRequestValidator : AbstractValidator<FlightStatusRequest>
{
    public FlightStatusRequestValidator()
    {
        RuleFor(x => x.FlightNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Flight number is required.")
            .Matches(@"^[A-Z]{2,3}\d{1,4}$").WithMessage("Flight number must consist of 2 to 3 uppercase letters followed by 1 to 4 digits (e.g., AI101).");

        RuleFor(x => x.DateStr)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Date is required.")
            .Matches(@"^\d{4}-\d{2}-\d{2}$").WithMessage("Date must be in 'yyyy-MM-dd' format.")
            .Must(BeAValidDate).WithMessage("Date must be a valid calendar date.");
    }

    private bool BeAValidDate(string dateStr)
    {
        return DateOnly.TryParseExact(dateStr, "yyyy-MM-dd", out _);
    }
}
