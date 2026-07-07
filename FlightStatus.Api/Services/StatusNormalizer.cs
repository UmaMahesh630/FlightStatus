namespace FlightStatus.Api.Services;

using FlightStatus.Api.Domain.Enums;

/// <summary>
/// Utility containing pure mapping functions for normalizing supplier status codes.
/// </summary>
/// <remarks>
/// Contains pure, stateless functions to translate supplier status codes to UnifiedFlightStatus.
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
