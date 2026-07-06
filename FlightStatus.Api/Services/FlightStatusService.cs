namespace FlightStatus.Api.Services;

using FlightStatus.Api.Domain.Enums;
using FlightStatus.Api.Domain.Models;
using FlightStatus.Api.Providers;
using Microsoft.Extensions.Logging;

/// <summary>
/// Default implementation of <see cref="IFlightStatusService"/> that orchestrates concurrent provider queries.
/// </summary>
/// <remarks>
/// ARCHITECTURE & DESIGN DECISIONS:
/// - **Scatter-Gather Pattern**: Queries multiple registered providers concurrently via <c>Task.WhenAll</c> 
///   to minimize API lookup latency (since network lookups can run in parallel).
/// - **Resilience / Graceful Degradation**: Individual provider failures are intercepted and logged using a try-catch blocks 
///   within the helper task. A single provider crash does not break the entire lookup operation (fault isolation).
/// - **Conflict Resolution Strategy**: Resolves status discrepancy by comparing the <c>LastUpdatedUtc</c> timestamp 
///   of each result and selecting the most recent update.
/// - **SOLID Principles**:
///   - **Single Responsibility Principle (SRP)**: This class only handles orchestration, logging, and conflict resolution. 
///     It has no knowledge of how raw provider DTOs are mapped or how network calls are made.
///   - **Open-Closed Principle (OCP)**: Scalable to any number of providers. New providers can be added without altering this class.
///   - **Dependency Inversion Principle (DIP)**: Depends entirely on abstractions (<c>IFlightStatusProvider</c> and <c>ILogger</c>).
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

        // Fire off lookup tasks to all registered providers concurrently
        var queryTasks = _providers.Select(provider => GetSafeStatusAsync(provider, flightNumber, date)).ToList();

        // Await completion of all parallel queries
        var rawResults = await Task.WhenAll(queryTasks);

        // Filter out null responses (failures or "Not Found" states)
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

        // Concurrency Conflict Resolution: Pick the one with the latest LastUpdatedUtc timestamp
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
            // Fail-safe requirement: If one provider fails, we log it and continue
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
            ScheduledDeparture = DateTime.UtcNow, // Set current time as placeholder
            ScheduledArrival = DateTime.UtcNow,   // Set current time as placeholder
            Terminal = null,
            Gate = null,
            DelayReason = reason,
            DataSource = "SystemFallback",
            LastUpdatedUtc = DateTime.UtcNow
        };
    }
}
