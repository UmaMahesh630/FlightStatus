using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace FlightStatus.Api.Dtos;

/// <summary>
/// Binds query parameters for the flight status request and defines Data Annotation rules.
/// </summary>
/// <remarks>
/// ARCHITECTURE & DESIGN DECISIONS:
/// - **AsParameters Bindings**: Uses the .NET 8 <see cref="AsParametersAttribute"/> to map individual query parameters 
///   into a single strongly typed parameter object, making endpoint signatures cleaner and more testable.
/// - **Data Annotations**: Implements declarative metadata rules like <see cref="RequiredAttribute"/> and 
///   <see cref="RegularExpressionAttribute"/>, serving as the first line of validation defense.
/// - **Separation of Concerns**: Decouples parameter binding/validation metadata from business logic structures.
/// </remarks>
public record FlightStatusRequest
{
    [FromQuery(Name = "flightNumber")]
    [Required(ErrorMessage = "Flight number is required.")]
    [RegularExpression(@"^[A-Z]{2,3}\d{1,4}$", ErrorMessage = "Flight number must consist of 2 to 3 uppercase letters followed by 1 to 4 digits (e.g., AI101).")]
    public string? FlightNumber { get; init; }

    [FromQuery(Name = "date")]
    [Required(ErrorMessage = "Date is required.")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Date must be in yyyy-MM-dd format.")]
    public string? DateStr { get; init; }
}
