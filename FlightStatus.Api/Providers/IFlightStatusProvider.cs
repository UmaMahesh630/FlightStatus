namespace FlightStatus.Api.Providers;

using FlightStatus.Api.Domain.Models;

/// <summary>
/// Defines the strategy interface for retrieving flight status from external data sources.
/// </summary>
/// <remarks>
/// ARCHITECTURE & DESIGN PATTERN: The Strategy Pattern
/// - **Why Chosen**: The Strategy Pattern defines a family of algorithms (how to fetch and normalize status from 
///   different vendors), encapsulates each one (e.g. AeroTrack vs. QuickFlight), and makes them interchangeable.
///   This allows the orchestration service to remain decoupled from vendor-specific details.
/// - **Open-Closed Principle (OCP)**: We can introduce a third or fourth flight provider (e.g. FlightRadar24) at 
///   any time by creating a new class implementing `IFlightStatusProvider` and registering it in DI, without 
///   having to modify existing orchestrator logic or endpoints.
/// - **Dependency Injection (DI)**: In .NET 8, registering multiple implementations of an interface allows us to 
///   inject an `IEnumerable<IFlightStatusProvider>` into the service layer, making it highly testable and configurable.
/// - **Asynchrony**: Methods return `Task` objects to ensure non-blocking, asynchronous execution during external 
///   I/O calls (simulating HTTP request/network waits).
/// </remarks>
public interface IFlightStatusProvider
{
    /// <summary>
    /// Gets the unique identifier/name of the provider (e.g., "AeroTrack", "QuickFlight").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Asynchronously queries the flight status from the provider and returns the normalized domain model.
    /// </summary>
    /// <param name="flightNumber">The flight code identifier.</param>
    /// <param name="date">The operating flight date.</param>
    /// <returns>
    /// A <see cref="FlightStatusResult"/> containing normalized status information, 
    /// or <c>null</c> if the flight is not found or the provider fails.
    /// </returns>
    Task<FlightStatusResult?> GetStatusAsync(string flightNumber, DateOnly date);
}
