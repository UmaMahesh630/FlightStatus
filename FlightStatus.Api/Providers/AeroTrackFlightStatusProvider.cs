namespace FlightStatus.Api.Providers;

using FlightStatus.Api.Domain.Enums;
using FlightStatus.Api.Domain.Models;
using FlightStatus.Api.Dtos;

/// <summary>
/// A deterministic stub implementation of <see cref="IFlightStatusProvider"/> simulating the AeroTrack data source.
/// </summary>
/// <remarks>
/// ARCHITECTURE & DESIGN DECISIONS:
/// - **Strategy Pattern Implementation**: Implements the strategy interface to supply AeroTrack-specific data fetching and normalization.
/// - **Anti-Corruption Layer (ACL)**: Raw AeroTrack DTOs (`AeroTrackResponse`) are processed and mapped into the internal domain model 
///   (`FlightStatusResult`) to prevent external naming schemes (e.g. "FlightCode", "LATE") from polluting internal systems.
/// - **Single Responsibility Principle (SRP)**: This class is solely responsible for modeling AeroTrack's data contract, simulating its retrieval, 
///   and performing its specific data transformation.
/// </remarks>
public class AeroTrackFlightStatusProvider : IFlightStatusProvider
{
    public string ProviderName => "AeroTrack";

    /// <summary>
    /// Simulates retrieving flight status from AeroTrack and normalizes it.
    /// </summary>
    public Task<FlightStatusResult?> GetStatusAsync(string flightNumber, DateOnly date)
    {
        // Normalize input for case-insensitive matching
        var normalizedFlightNum = flightNumber.Trim().ToUpperInvariant();

        // Simulate database/API retrieval of raw vendor data
        var rawResponse = GetRawMockData(normalizedFlightNum, date);

        if (rawResponse == null)
        {
            return Task.FromResult<FlightStatusResult?>(null);
        }

        // Map raw response to unified domain model
        var result = MapToUnified(rawResponse);
        return Task.FromResult<FlightStatusResult?>(result);
    }

    /// <summary>
    /// Mock database of AeroTrack's proprietary schema.
    /// </summary>
    private AeroTrackResponse? GetRawMockData(string flightNum, DateOnly date)
    {
        return flightNum switch
        {
            "AI101" => new AeroTrackResponse
            {
                FlightCode = "AI101",
                OperatingDate = date.ToString("yyyy-MM-dd"),
                Status = "ON_TIME",
                ScheduledDeparture = date.ToDateTime(new TimeOnly(10, 0, 0)),
                ActualDeparture = date.ToDateTime(new TimeOnly(10, 5, 0)), // 5 mins late (On Time)
                ScheduledArrival = date.ToDateTime(new TimeOnly(18, 0, 0)),
                ActualArrival = date.ToDateTime(new TimeOnly(18, 2, 0)),
                DepartureTerminal = "T3",
                DepartureGate = "Gate A1",
                ArrivalTerminal = "T1",
                ArrivalGate = "Gate B4",
                DelayReason = null,
                LastUpdated = date.ToDateTime(new TimeOnly(11, 0, 0)) // 11:00 AM UTC update
            },
            "BA202" => new AeroTrackResponse
            {
                FlightCode = "BA202",
                OperatingDate = date.ToString("yyyy-MM-dd"),
                Status = "LATE",
                ScheduledDeparture = date.ToDateTime(new TimeOnly(14, 0, 0)),
                ActualDeparture = date.ToDateTime(new TimeOnly(15, 30, 0)), // 90 mins late (Delayed)
                ScheduledArrival = date.ToDateTime(new TimeOnly(20, 0, 0)),
                ActualArrival = date.ToDateTime(new TimeOnly(21, 15, 0)),
                DepartureTerminal = "T5",
                DepartureGate = "Gate B22",
                ArrivalTerminal = "T3",
                ArrivalGate = "Gate C1",
                DelayReason = "Late incoming aircraft due to weather conditions",
                LastUpdated = date.ToDateTime(new TimeOnly(15, 45, 0)) // 3:45 PM UTC update
            },
            "UA303" => new AeroTrackResponse
            {
                FlightCode = "UA303",
                OperatingDate = date.ToString("yyyy-MM-dd"),
                Status = "CANCELLED",
                ScheduledDeparture = date.ToDateTime(new TimeOnly(17, 30, 0)),
                ActualDeparture = null,
                ScheduledArrival = date.ToDateTime(new TimeOnly(23, 45, 0)),
                ActualArrival = null,
                DepartureTerminal = "T1",
                DepartureGate = "Gate C10",
                ArrivalTerminal = "T2",
                ArrivalGate = "Gate A9",
                DelayReason = "Technical crew availability issue",
                LastUpdated = date.ToDateTime(new TimeOnly(16, 30, 0)) // 4:30 PM UTC update
            },
            _ => null
        };
    }

    /// <summary>
    /// Normalization helper to map AeroTrackResponse to Unified FlightStatusResult.
    /// </summary>
    private FlightStatusResult MapToUnified(AeroTrackResponse raw)
    {
        return new FlightStatusResult
        {
            FlightNumber = raw.FlightCode,
            Date = DateOnly.Parse(raw.OperatingDate),
            Status = MapStatus(raw.Status),
            ScheduledDeparture = raw.ScheduledDeparture,
            ActualDeparture = raw.ActualDeparture,
            ScheduledArrival = raw.ScheduledArrival,
            ActualArrival = raw.ActualArrival,
            Terminal = raw.DepartureTerminal, // Mapping terminal
            Gate = raw.DepartureGate,           // Mapping gate
            DelayReason = raw.DelayReason,     // Mapping delay reason
            DataSource = ProviderName,
            LastUpdatedUtc = DateTime.SpecifyKind(raw.LastUpdated, DateTimeKind.Utc)
        };
    }

    /// <summary>
    /// Normalization rules to map AeroTrack status strings to UnifiedFlightStatus enum values.
    /// </summary>
    private static UnifiedFlightStatus MapStatus(string rawStatus)
    {
        return rawStatus switch
        {
            "ON_TIME" => UnifiedFlightStatus.OnTime,
            "LATE" => UnifiedFlightStatus.Delayed,
            "CANCELLED" => UnifiedFlightStatus.Cancelled,
            "DIVERTED" => UnifiedFlightStatus.Diverted,
            _ => UnifiedFlightStatus.Unknown
        };
    }
}
