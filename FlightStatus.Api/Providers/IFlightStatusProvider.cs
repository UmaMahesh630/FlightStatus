namespace FlightStatus.Api.Providers;

using FlightStatus.Api.Domain.Models;

/// <summary>
/// Strategy contract for external flight status suppliers.
/// </summary>
/// <remarks>
/// Encapsulates supplier-specific data retrieval and normalization. Concrete strategies 
/// map raw vendor schemas to the unified domain models.
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
