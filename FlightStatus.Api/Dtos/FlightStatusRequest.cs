using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace FlightStatus.Api.Dtos;

/// <summary>
/// Parameter record bundling incoming GET query arguments.
/// </summary>
/// <remarks>
/// Binds individual query parameters into a strongly-typed object. Basic presence check 
/// is validated by Data Annotations, while format validation is delegated to FluentValidation.
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
