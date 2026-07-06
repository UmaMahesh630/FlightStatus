namespace FlightStatus.Api.Services;

using FlightStatus.Api.Domain.Enums;

/// <summary>
/// A utility service containing pure mapping functions for normalizing flight status strings from multiple providers.
/// </summary>
/// <remarks>
/// ARCHITECTURE & DESIGN DECISIONS:
/// - **Pure Functions / Static Class**: Status normalization is side-effect free, deterministic, and stateless. 
///   Using a static class with pure functions avoids unnecessary dependency injection overhead, eliminates heap 
///   allocations, and simplifies testing (no mocking of normalizer is required).
/// - **Alternative (Injected Service)**: Registering an `IFlightStatusNormalizer` in DI. This is an alternative if 
///   status rules were dynamically loaded from a database. For static, compilation-bound rules, pure functions 
///   are simpler and more performant.
/// - **Single Responsibility Principle (SRP)**: This class is the single source of truth for converting provider-specific 
///   status vocabularies. If a provider changes their status codes, this is the only class that needs to change.
/// </remarks>
public static class StatusNormalizer
{
    /// <summary>
    /// Normalizes AeroTrack statuses (ON_TIME, LATE, CANCELLED, DIVERTED) to UnifiedFlightStatus.
    /// </summary>
    public static UnifiedFlightStatus MapAeroTrackStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return UnifiedFlightStatus.Unknown;
        }

        return status.Trim().ToUpperInvariant() switch
        {
            "ON_TIME" => UnifiedFlightStatus.OnTime,
            "LATE" => UnifiedFlightStatus.Delayed,
            "CANCELLED" => UnifiedFlightStatus.Cancelled,
            "DIVERTED" => UnifiedFlightStatus.Diverted,
            _ => UnifiedFlightStatus.Unknown
        };
    }

    /// <summary>
    /// Normalizes QuickFlight statuses (ON_SCHEDULE, DELAYED, CANCELED, REROUTED) to UnifiedFlightStatus.
    /// </summary>
    public static UnifiedFlightStatus MapQuickFlightStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return UnifiedFlightStatus.Unknown;
        }

        return status.Trim().ToUpperInvariant() switch
        {
            "ON_SCHEDULE" => UnifiedFlightStatus.OnTime,
            "DELAYED" => UnifiedFlightStatus.Delayed,
            "CANCELED" => UnifiedFlightStatus.Cancelled,
            "REROUTED" => UnifiedFlightStatus.Diverted,
            _ => UnifiedFlightStatus.Unknown
        };
    }
}
