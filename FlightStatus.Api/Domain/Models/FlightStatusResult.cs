namespace FlightStatus.Api.Domain.Models;

using FlightStatus.Api.Domain.Enums;

/// <summary>
/// Normalized flight status domain model.
/// </summary>
/// <remarks>
/// Implemented as an immutable record to ensure thread safety during concurrent 
/// provider queries and prevent side-effects in downstream mappers.
/// </remarks>
public record FlightStatusResult
{
    public required string FlightNumber { get; init; }
    public required DateOnly Date { get; init; }
    public required UnifiedFlightStatus Status { get; init; }
    public string StatusText => Status.ToString();
    
    public required DateTime ScheduledDeparture { get; init; }
    public DateTime? ActualDeparture { get; init; }
    public required DateTime ScheduledArrival { get; init; }
    public DateTime? ActualArrival { get; init; }

    // AeroTrack-specific fields (null if absent or if QuickFlight is chosen)
    public string? Terminal { get; init; }
    public string? Gate { get; init; }
    public string? DelayReason { get; init; }

    // Metadata tracing fields
    public required string DataSource { get; init; }
    public required DateTime LastUpdatedUtc { get; init; }
}
