namespace FlightStatus.Api.Providers;

using FlightStatus.Api.Domain.Enums;
using FlightStatus.Api.Domain.Models;
using FlightStatus.Api.Dtos;

/// <summary>
/// A deterministic stub implementation of <see cref="IFlightStatusProvider"/> simulating the QuickFlight data source.
/// </summary>
/// <remarks>
/// ARCHITECTURE & DESIGN DECISIONS:
/// - **Strategy Pattern Implementation**: Implements the strategy interface to supply QuickFlight-specific data fetching and normalization.
/// - **Anti-Corruption Layer (ACL)**: Raw vendor responses (`QuickFlightResponse`) are mapped directly into the internal domain model 
///   (`FlightStatusResult`) to prevent external naming schemes (e.g. "FlightNum", "CX") from polluting core systems.
/// - **Minimal Data Footprint**: Adheres to the QuickFlight specification by leaving `Terminal`, `Gate`, and `DelayReason` as `null` (QuickFlight does not capture these).
/// - **Conflict Resolution Test Setup**: Timestamps are strategically offset from `AeroTrackFlightStatusProvider` to allow the orchestration service to resolve conflicts based on the latest update:
///   - AI101: QuickFlight is newer (11:15 AM vs 11:00 AM)
///   - BA202: AeroTrack is newer (3:45 PM vs 3:30 PM)
///   - UA303: AeroTrack is newer (4:30 PM vs 4:00 PM)
/// </remarks>
public class QuickFlightFlightStatusProvider : IFlightStatusProvider
{
    public string ProviderName => "QuickFlight";

    /// <summary>
    /// Simulates retrieving flight status from QuickFlight and normalizes it.
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
    /// Mock database of QuickFlight's proprietary minimal schema.
    /// </summary>
    private QuickFlightResponse? GetRawMockData(string flightNum, DateOnly date)
    {
        return flightNum switch
        {
            "AI101" => new QuickFlightResponse
            {
                FlightNum = "AI101",
                Date = date.ToString("yyyy-MM-dd"),
                StatusCode = "OK",
                ScheduledDep = date.ToDateTime(new TimeOnly(10, 0, 0)),
                ScheduledArr = date.ToDateTime(new TimeOnly(18, 0, 0)),
                UpdatedAtUtc = date.ToDateTime(new TimeOnly(11, 15, 0)) // 11:15 AM UTC (newer than AeroTrack's 11:00 AM)
            },
            "BA202" => new QuickFlightResponse
            {
                FlightNum = "BA202",
                Date = date.ToString("yyyy-MM-dd"),
                StatusCode = "DELAY",
                ScheduledDep = date.ToDateTime(new TimeOnly(14, 0, 0)),
                ScheduledArr = date.ToDateTime(new TimeOnly(20, 0, 0)),
                UpdatedAtUtc = date.ToDateTime(new TimeOnly(15, 30, 0)) // 3:30 PM UTC (older than AeroTrack's 3:45 PM)
            },
            "UA303" => new QuickFlightResponse
            {
                FlightNum = "UA303",
                Date = date.ToString("yyyy-MM-dd"),
                StatusCode = "CX",
                ScheduledDep = date.ToDateTime(new TimeOnly(17, 30, 0)),
                ScheduledArr = date.ToDateTime(new TimeOnly(23, 45, 0)),
                UpdatedAtUtc = date.ToDateTime(new TimeOnly(16, 0, 0)) // 4:00 PM UTC (older than AeroTrack's 4:30 PM)
            },
            _ => null
        };
    }

    /// <summary>
    /// Normalization helper to map QuickFlightResponse to Unified FlightStatusResult.
    /// </summary>
    private FlightStatusResult MapToUnified(QuickFlightResponse raw)
    {
        return new FlightStatusResult
        {
            FlightNumber = raw.FlightNum,
            Date = DateOnly.Parse(raw.Date),
            Status = MapStatus(raw.StatusCode),
            ScheduledDeparture = raw.ScheduledDep,
            ActualDeparture = null, // QuickFlight doesn't return actual times
            ScheduledArrival = raw.ScheduledArr,
            ActualArrival = null,   // QuickFlight doesn't return actual times
            Terminal = null,        // QuickFlight doesn't support terminals
            Gate = null,            // QuickFlight doesn't support gates
            DelayReason = null,     // QuickFlight doesn't support delay reasons
            DataSource = ProviderName,
            LastUpdatedUtc = DateTime.SpecifyKind(raw.UpdatedAtUtc, DateTimeKind.Utc)
        };
    }

    /// <summary>
    /// Normalization rules to map QuickFlight status strings to UnifiedFlightStatus enum values.
    /// </summary>
    private static UnifiedFlightStatus MapStatus(string statusCode)
    {
        return statusCode switch
        {
            "OK" => UnifiedFlightStatus.OnTime,
            "DELAY" => UnifiedFlightStatus.Delayed,
            "CX" => UnifiedFlightStatus.Cancelled,
            "DIV" => UnifiedFlightStatus.Diverted,
            _ => UnifiedFlightStatus.Unknown
        };
    }
}
