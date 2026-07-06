namespace FlightStatus.Api.Domain.Models;

using FlightStatus.Api.Domain.Enums;

/// <summary>
/// Represents the normalized flight status domain model returned by the service layer and Minimal API.
/// </summary>
/// <remarks>
/// ARCHITECTURE & DESIGN DECISION: Immutability and Thread-Safety via C# Records
/// - **Strategy**: Value Object / Data Transfer Object (DTO) pattern.
/// - **Rationale**: FlightStatusResult is a read-only transfer model. Using a C# `record` enforces immutability 
///   by default, preventing accidental mutations as it propagates through our services, mappers, and API responses. 
///   It also provides built-in value-based equality and compiler-generated formatting, reducing boilerplate.
/// - **Alternative**: Standard classes with getter/setter properties. However, standard classes allow side-effects 
///   and mutations which can introduce bugs in multi-threaded/concurrent code (like our parallel provider query).
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
