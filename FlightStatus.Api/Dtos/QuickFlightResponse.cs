namespace FlightStatus.Api.Dtos;

/// <summary>
/// Raw Data Transfer Object capturing the response schema from the QuickFlight supplier.
/// </summary>
/// <remarks>
/// Serves as part of the Anti-Corruption Layer (ACL), isolating QuickFlight's custom schema 
/// from the application domain.
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
