namespace FlightStatus.Api.Providers;

using FlightStatus.Api.Domain.Enums;
using FlightStatus.Api.Domain.Models;
using FlightStatus.Api.Dtos;
using FlightStatus.Api.Services;

/// <summary>
/// QuickFlight supplier implementation.
/// </summary>
/// <remarks>
/// Simulates QuickFlight's minimal schema, leaving terminal/gate info null and 
/// using strategically offset update timestamps for testing conflict resolution.
/// </remarks>
public class QuickFlightFlightStatusProvider : IFlightStatusProvider
{
    public string ProviderName => "QuickFlight";

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

    private QuickFlightResponse? GetRawMockData(string flightNum, DateOnly date)
    {
        return flightNum switch
        {
            "AI101" => new QuickFlightResponse
            {
                FlightNum = "AI101",
                Date = date.ToString("yyyy-MM-dd"),
                StatusCode = "ON_SCHEDULE",
                ScheduledDep = date.ToDateTime(new TimeOnly(10, 0, 0)),
                ScheduledArr = date.ToDateTime(new TimeOnly(18, 0, 0)),
                UpdatedAtUtc = date.ToDateTime(new TimeOnly(11, 15, 0))
            },
            "BA202" => new QuickFlightResponse
            {
                FlightNum = "BA202",
                Date = date.ToString("yyyy-MM-dd"),
                StatusCode = "DELAYED",
                ScheduledDep = date.ToDateTime(new TimeOnly(14, 0, 0)),
                ScheduledArr = date.ToDateTime(new TimeOnly(20, 0, 0)),
                UpdatedAtUtc = date.ToDateTime(new TimeOnly(15, 30, 0))
            },
            "UA303" => new QuickFlightResponse
            {
                FlightNum = "UA303",
                Date = date.ToString("yyyy-MM-dd"),
                StatusCode = "CANCELED",
                ScheduledDep = date.ToDateTime(new TimeOnly(17, 30, 0)),
                ScheduledArr = date.ToDateTime(new TimeOnly(23, 45, 0)),
                UpdatedAtUtc = date.ToDateTime(new TimeOnly(16, 0, 0))
            },
            _ => null
        };
    }

    private FlightStatusResult MapToUnified(QuickFlightResponse raw)
    {
        return new FlightStatusResult
        {
            FlightNumber = raw.FlightNum,
            Date = DateOnly.Parse(raw.Date),
            Status = StatusNormalizer.MapQuickFlightStatus(raw.StatusCode),
            ScheduledDeparture = raw.ScheduledDep,
            ActualDeparture = null,
            ScheduledArrival = raw.ScheduledArr,
            ActualArrival = null,
            Terminal = null,
            Gate = null,
            DelayReason = null,
            DataSource = ProviderName,
            LastUpdatedUtc = DateTime.SpecifyKind(raw.UpdatedAtUtc, DateTimeKind.Utc)
        };
    }
}
