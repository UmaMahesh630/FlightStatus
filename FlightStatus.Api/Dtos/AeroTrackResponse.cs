namespace FlightStatus.Api.Dtos;

/// <summary>
/// Raw Data Transfer Object (DTO) capturing the verbose response schema from the AeroTrack provider.
/// </summary>
/// <remarks>
/// ARCHITECTURE & DESIGN DECISION: Interface Boundary & Anti-Corruption Layer (ACL)
/// - **Strategy**: Data Transfer Object (DTO) pattern.
/// - **Rationale**: External vendor contracts are unstable and outside our control. If AeroTrack changes its schema, 
///   only this DTO and its corresponding provider adapter require changes. This insulates our core domain model 
///   from external volatility (Anti-Corruption Layer).
/// - **Alternative**: Reusing the domain model (`FlightStatusResult`) to deserialize vendor responses directly. 
///   This would tightly couple our API contracts to third-party providers, violating the Single Responsibility Principle 
///   and making it hard to add or drop vendors.
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
