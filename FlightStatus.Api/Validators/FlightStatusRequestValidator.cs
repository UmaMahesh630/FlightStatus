using FluentValidation;
using FlightStatus.Api.Dtos;

namespace FlightStatus.Api.Validators;

/// <summary>
/// Defines advanced business validation rules for <see cref="FlightStatusRequest"/> using FluentValidation.
/// </summary>
/// <remarks>
/// ARCHITECTURE & DESIGN DECISIONS:
/// - **Fluent Validation**: Chosen for complex business rules because it is fluent, strongly typed, and separates 
///   validation logic completely from the model definitions, making validation rules easier to read, write, and unit test.
/// - **Rule Segregation**: Validates syntax format (Regex) and performs logical state checks (Date parsing correctness).
/// - **SOLID - Single Responsibility Principle (SRP)**: This validator is only responsible for the logic rules of the 
///   FlightStatusRequest, ensuring business models stay pure.
/// </remarks>
public class FlightStatusRequestValidator : AbstractValidator<FlightStatusRequest>
{
    public FlightStatusRequestValidator()
    {
        RuleFor(x => x.FlightNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Flight number is required.")
            .Matches(@"^[a-zA-Z0-9]{3,10}$").WithMessage("Flight number must be alphanumeric and between 3 and 10 characters.");

        RuleFor(x => x.DateStr)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Date is required.")
            .Matches(@"^\d{4}-\d{2}-\d{2}$").WithMessage("Date must be in 'yyyy-MM-dd' format.")
            .Must(BeAValidDate).WithMessage("Date must be a valid calendar date.");
    }

    /// <summary>
    /// Logical calendar validation to prevent dates like February 30th from passing validation.
    /// </summary>
    private bool BeAValidDate(string dateStr)
    {
        return DateOnly.TryParseExact(dateStr, "yyyy-MM-dd", out _);
    }
}
