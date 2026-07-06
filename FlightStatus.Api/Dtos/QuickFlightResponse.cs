namespace FlightStatus.Api.Dtos;

/// <summary>
/// Raw Data Transfer Object (DTO) capturing the minimal response schema from the QuickFlight provider.
/// </summary>
/// <remarks>
/// ARCHITECTURE & DESIGN DECISION: Interface Boundary & Anti-Corruption Layer (ACL)
/// - **Strategy**: Data Transfer Object (DTO) pattern.
/// - **Rationale**: Like AeroTrackResponse, this isolates the vendor-specific minimal contract. It lacks terminal, 
///   gate, and delay details, highlighting why we map vendor DTOs into a unified model where these fields are 
///   optional (nullable).
/// - **Alternative**: Forcing QuickFlight to return terminal and gate fields containing placeholder/dummy values. 
///   Instead, mapping it to a unified class with nullable fields naturally models vendor variance.
/// </remarks>
public record QuickFlightResponse
{
    public string FlightNum { get; init; } = string.Empty;
    public string Date { get; init; } = string.Empty;
    public string StatusCode { get; init; } = string.Empty; // e.g., "OK", "DELAY", "CX", "DIV"
    public DateTime ScheduledDep { get; init; }
    public DateTime ScheduledArr { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
}
