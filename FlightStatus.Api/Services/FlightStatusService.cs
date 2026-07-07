namespace FlightStatus.Api.Services;

using FlightStatus.Api.Domain.Enums;
using FlightStatus.Api.Domain.Models;
using FlightStatus.Api.Providers;
using Microsoft.Extensions.Logging;

/// <summary>
/// Orchestrates concurrent queries to all registered suppliers.
/// </summary>
/// <remarks>
/// Queries multiple suppliers in parallel via Task.WhenAll. Resolves conflict records 
/// by selecting the result with the latest LastUpdatedUtc timestamp.
/// </remarks>
public class FlightStatusService : IFlightStatusService
{
    private readonly IEnumerable<IFlightStatusProvider> _providers;
    private readonly ILogger<FlightStatusService> _logger;

    public FlightStatusService(IEnumerable<IFlightStatusProvider> providers, ILogger<FlightStatusService> logger)
    {
        _providers = providers ?? throw new ArgumentNullException(nameof(providers));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<FlightStatusResult> ExecuteLookupAsync(string flightNumber, DateOnly date)
    {
        if (string.IsNullOrWhiteSpace(flightNumber))
        {
            _logger.LogWarning("Execution aborted: Flight number parameter is null or empty.");
            throw new ArgumentException("Flight number must be provided.", nameof(flightNumber));
        }

        _logger.LogInformation("Beginning flight status lookup for {FlightNumber} on date {Date}", flightNumber, date);

        if (!_providers.Any())
        {
            _logger.LogError("System configuration error: No flight status providers are registered in the DI container.");
            return CreateUnknownFallback(flightNumber, date, "No providers configured in the system.");
        }

        var queryTasks = _providers.Select(provider => GetSafeStatusAsync(provider, flightNumber, date)).ToList();
        var rawResults = await Task.WhenAll(queryTasks);
        var validResults = rawResults.Where(r => r != null).Cast<FlightStatusResult>().ToList();

        if (validResults.Count == 0)
        {
            _logger.LogWarning("No flight status details could be retrieved from any provider for flight {FlightNumber} on {Date}", flightNumber, date);
            return CreateUnknownFallback(flightNumber, date, "Flight status currently unavailable from all providers.");
        }

        if (validResults.Count == 1)
        {
            var singleResult = validResults[0];
            _logger.LogInformation("Single provider response resolved. Selected {ProviderName} for flight {FlightNumber}.", singleResult.DataSource, flightNumber);
            return singleResult;
        }

        var selectedResult = validResults.OrderByDescending(r => r.LastUpdatedUtc).First();

        _logger.LogInformation("Multiple providers responded. Selection algorithm picked {SelectedProvider} with later timestamp {Timestamp} (vs other provider updates).", 
            selectedResult.DataSource, selectedResult.LastUpdatedUtc);

        return selectedResult;
    }

    /// <summary>
    /// Safely wraps a provider call in a try-catch block to insulate other provider lookups from exceptions.
    /// </summary>
    private async Task<FlightStatusResult?> GetSafeStatusAsync(IFlightStatusProvider provider, string flightNumber, DateOnly date)
    {
        try
        {
            _logger.LogDebug("Querying provider {ProviderName} for flight {FlightNumber}...", provider.ProviderName, flightNumber);
            var result = await provider.GetStatusAsync(flightNumber, date);
            
            if (result == null)
            {
                _logger.LogDebug("Provider {ProviderName} returned no record for flight {FlightNumber} on {Date}.", provider.ProviderName, flightNumber, date);
            }
            else
            {
                _logger.LogDebug("Provider {ProviderName} returned status {Status} updated at {UpdatedAtUtc} UTC.", provider.ProviderName, result.Status, result.LastUpdatedUtc);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while querying provider {ProviderName} for flight {FlightNumber}.", provider.ProviderName, flightNumber);
            return null;
        }
    }

    /// <summary>
    /// Helper to generate a fallback object when all providers fail or return null.
    /// </summary>
    private static FlightStatusResult CreateUnknownFallback(string flightNumber, DateOnly date, string reason)
    {
        return new FlightStatusResult
        {
            FlightNumber = flightNumber,
            Date = date,
            Status = UnifiedFlightStatus.Unknown,
            ScheduledDeparture = DateTime.UtcNow,
            ScheduledArrival = DateTime.UtcNow,
            Terminal = null,
            Gate = null,
            DelayReason = reason,
            DataSource = "SystemFallback",
            LastUpdatedUtc = DateTime.UtcNow
        };
    }
}
