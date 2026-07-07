namespace FlightStatus.Api.Providers;

using FlightStatus.Api.Domain.Enums;
using FlightStatus.Api.Domain.Models;
using FlightStatus.Api.Dtos;
using FlightStatus.Api.Services;

/// <summary>
/// AeroTrack supplier implementation.
/// </summary>
/// <remarks>
/// Simulates AeroTrack's proprietary schema, mapping responses to the unified domain record.
/// </remarks>
public class AeroTrackFlightStatusProvider : IFlightStatusProvider
{
    public string ProviderName => "AeroTrack";

    public Task<FlightStatusResult?> GetStatusAsync(string flightNumber, DateOnly date)
    {
        var normalizedFlightNum = flightNumber.Trim().ToUpperInvariant();
        var rawResponse = GetRawMockData(normalizedFlightNum, date);

        if (rawResponse == null)
        {
            return Task.FromResult<FlightStatusResult?>(null);
        }

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
                ActualDeparture = date.ToDateTime(new TimeOnly(10, 5, 0)),
                ScheduledArrival = date.ToDateTime(new TimeOnly(18, 0, 0)),
                ActualArrival = date.ToDateTime(new TimeOnly(18, 2, 0)),
                DepartureTerminal = "T3",
                DepartureGate = "Gate A1",
                ArrivalTerminal = "T1",
                ArrivalGate = "Gate B4",
                DelayReason = null,
                LastUpdated = date.ToDateTime(new TimeOnly(11, 0, 0))
            },
            "BA202" => new AeroTrackResponse
            {
                FlightCode = "BA202",
                OperatingDate = date.ToString("yyyy-MM-dd"),
                Status = "LATE",
                ScheduledDeparture = date.ToDateTime(new TimeOnly(14, 0, 0)),
                ActualDeparture = date.ToDateTime(new TimeOnly(15, 30, 0)),
                ScheduledArrival = date.ToDateTime(new TimeOnly(20, 0, 0)),
                ActualArrival = date.ToDateTime(new TimeOnly(21, 15, 0)),
                DepartureTerminal = "T5",
                DepartureGate = "Gate B22",
                ArrivalTerminal = "T3",
                ArrivalGate = "Gate C1",
                DelayReason = "Late incoming aircraft due to weather conditions",
                LastUpdated = date.ToDateTime(new TimeOnly(15, 45, 0))
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
                LastUpdated = date.ToDateTime(new TimeOnly(16, 30, 0))
            },
            _ => null
        };
    }

    private FlightStatusResult MapToUnified(AeroTrackResponse raw)
    {
        return new FlightStatusResult
        {
            FlightNumber = raw.FlightCode,
            Date = DateOnly.Parse(raw.OperatingDate),
            Status = StatusNormalizer.MapAeroTrackStatus(raw.Status),
            ScheduledDeparture = raw.ScheduledDeparture,
            ActualDeparture = raw.ActualDeparture,
            ScheduledArrival = raw.ScheduledArrival,
            ActualArrival = raw.ActualArrival,
            Terminal = raw.DepartureTerminal,
            Gate = raw.DepartureGate,
            DelayReason = raw.DelayReason,
            DataSource = ProviderName,
            LastUpdatedUtc = DateTime.SpecifyKind(raw.LastUpdated, DateTimeKind.Utc)
        };
    }
}
