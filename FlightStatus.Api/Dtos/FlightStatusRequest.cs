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
    public string? FlightNumber { get; init; }

    [FromQuery(Name = "date")]
    [Required(ErrorMessage = "Date is required.")]
    public string? DateStr { get; init; }
}
