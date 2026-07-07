namespace FlightStatus.Api.Services;

using FlightStatus.Api.Domain.Models;

/// <summary>
/// Service coordinating flight status queries across multiple external data providers.
/// </summary>
/// <remarks>
/// Acts as the orchestration layer boundary, fetching vendor details in parallel and 
/// resolving timing conflicts.
/// </remarks>
public interface IFlightStatusService
{
    /// <summary>
    /// Executes the parallel lookup across all providers, normalizes results, and determines the best status response.
    /// </summary>
    /// <param name="flightNumber">The flight code to search.</param>
    /// <param name="date">The flight operating date.</param>
    /// <returns>A normalized <see cref="FlightStatusResult"/>, possibly with Unknown status if no providers respond.</returns>
    Task<FlightStatusResult> ExecuteLookupAsync(string flightNumber, DateOnly date);
}
