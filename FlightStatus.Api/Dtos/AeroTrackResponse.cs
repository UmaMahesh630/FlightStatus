namespace FlightStatus.Api.Dtos;

/// <summary>
/// Raw Data Transfer Object capturing the response schema from the AeroTrack supplier.
/// </summary>
/// <remarks>
/// Serves as part of the Anti-Corruption Layer (ACL). Insulates core domain models 
/// from third-party vendor schema volatility.
/// </remarks>
public record AeroTrackResponse
{
    public string FlightCode { get; init; } = string.Empty;
    public string OperatingDate { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty; // e.g., "ON_TIME", "LATE", "CANCELLED", "DIVERTED"
    public DateTime ScheduledDeparture { get; init; }
    public DateTime? ActualDeparture { get; init; }
    public DateTime ScheduledArrival { get; init; }
    public DateTime? ActualArrival { get; init; }
    public string DepartureTerminal { get; init; } = string.Empty;
    public string DepartureGate { get; init; } = string.Empty;
    public string ArrivalTerminal { get; init; } = string.Empty;
    public string ArrivalGate { get; init; } = string.Empty;
    public string? DelayReason { get; init; }
    public DateTime LastUpdated { get; init; }
}
