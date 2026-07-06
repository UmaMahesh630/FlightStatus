namespace FlightStatus.Api.Services;

using FlightStatus.Api.Domain.Models;

/// <summary>
/// Service coordinating flight status queries across multiple external data providers.
/// </summary>
/// <remarks>
/// ARCHITECTURE & DESIGN DECISIONS:
/// - **Orchestration Service Layer**: Acting as a single entry point for API endpoints to perform business processes.
/// - **SOLID - Dependency Inversion Principle (DIP)**: Endpoints depend on this interface, not the concrete implementation.
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
