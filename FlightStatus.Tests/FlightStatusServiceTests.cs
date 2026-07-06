using FlightStatus.Api.Domain.Enums;
using FlightStatus.Api.Domain.Models;
using FlightStatus.Api.Providers;
using FlightStatus.Api.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace FlightStatus.Tests;

public class FlightStatusServiceTests
{
    private readonly IFlightStatusProvider _provider1;
    private readonly IFlightStatusProvider _provider2;
    private readonly ILogger<FlightStatusService> _logger;
    private readonly FlightStatusService _sut; // System Under Test

    public FlightStatusServiceTests()
    {
        _provider1 = Substitute.For<IFlightStatusProvider>();
        _provider2 = Substitute.For<IFlightStatusProvider>();
        _logger = Substitute.For<ILogger<FlightStatusService>>();

        _provider1.ProviderName.Returns("AeroTrack");
        _provider2.ProviderName.Returns("QuickFlight");

        // Inject the mocked providers list
        _sut = new FlightStatusService(new[] { _provider1, _provider2 }, _logger);
    }

    [Fact]
    public async Task ExecuteLookupAsync_WhenBothProvidersRespond_ShouldSelectLatestLastUpdatedUtc()
    {
        // Arrange
        var flightNumber = "AI101";
        var date = new DateOnly(2026, 7, 6);

        var olderResult = new FlightStatusResult
        {
            FlightNumber = flightNumber,
            Date = date,
            Status = UnifiedFlightStatus.OnTime,
            ScheduledDeparture = DateTime.UtcNow,
            ScheduledArrival = DateTime.UtcNow,
            DataSource = "AeroTrack",
            LastUpdatedUtc = new DateTime(2026, 7, 6, 11, 0, 0, DateTimeKind.Utc)
        };

        var newerResult = new FlightStatusResult
        {
            FlightNumber = flightNumber,
            Date = date,
            Status = UnifiedFlightStatus.Delayed, // Different status to verify selection
            ScheduledDeparture = DateTime.UtcNow,
            ScheduledArrival = DateTime.UtcNow,
            DataSource = "QuickFlight",
            LastUpdatedUtc = new DateTime(2026, 7, 6, 11, 15, 0, DateTimeKind.Utc) // Newer timestamp
        };

        _provider1.GetStatusAsync(flightNumber, date).Returns(olderResult);
        _provider2.GetStatusAsync(flightNumber, date).Returns(newerResult);

        // Act
        var result = await _sut.ExecuteLookupAsync(flightNumber, date);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("QuickFlight", result.DataSource);
        Assert.Equal(UnifiedFlightStatus.Delayed, result.Status);
        Assert.Equal(newerResult.LastUpdatedUtc, result.LastUpdatedUtc);
    }

    [Fact]
    public async Task ExecuteLookupAsync_WhenOnlyOneProviderResponds_ShouldReturnSuccessfulProviderResult()
    {
        // Arrange
        var flightNumber = "BA202";
        var date = new DateOnly(2026, 7, 6);

        var successfulResult = new FlightStatusResult
        {
            FlightNumber = flightNumber,
            Date = date,
            Status = UnifiedFlightStatus.Delayed,
            ScheduledDeparture = DateTime.UtcNow,
            ScheduledArrival = DateTime.UtcNow,
            DataSource = "AeroTrack",
            LastUpdatedUtc = DateTime.UtcNow
        };

        _provider1.GetStatusAsync(flightNumber, date).Returns(successfulResult);
        _provider2.GetStatusAsync(flightNumber, date).Returns((FlightStatusResult?)null); // QuickFlight returns null

        // Act
        var result = await _sut.ExecuteLookupAsync(flightNumber, date);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("AeroTrack", result.DataSource);
        Assert.Equal(UnifiedFlightStatus.Delayed, result.Status);
    }

    [Fact]
    public async Task ExecuteLookupAsync_WhenOneProviderFails_ShouldGracefullyContinueAndReturnSuccessResult()
    {
        // Arrange
        var flightNumber = "UA303";
        var date = new DateOnly(2026, 7, 6);

        var successfulResult = new FlightStatusResult
        {
            FlightNumber = flightNumber,
            Date = date,
            Status = UnifiedFlightStatus.Cancelled,
            ScheduledDeparture = DateTime.UtcNow,
            ScheduledArrival = DateTime.UtcNow,
            DataSource = "AeroTrack",
            LastUpdatedUtc = DateTime.UtcNow
        };

        // Mock provider 1 succeeding and provider 2 throwing an exception
        _provider1.GetStatusAsync(flightNumber, date).Returns(successfulResult);
        _provider2.GetStatusAsync(flightNumber, date).Throws(new Exception("Network failure simulated"));

        // Act
        var result = await _sut.ExecuteLookupAsync(flightNumber, date);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("AeroTrack", result.DataSource);
        Assert.Equal(UnifiedFlightStatus.Cancelled, result.Status);
    }

    [Fact]
    public async Task ExecuteLookupAsync_WhenBothProvidersFail_ShouldReturnUnknownStatusFallback()
    {
        // Arrange
        var flightNumber = "LH404";
        var date = new DateOnly(2026, 7, 6);

        // Mock both providers throwing exceptions
        _provider1.GetStatusAsync(flightNumber, date).Throws(new Exception("AeroTrack crash"));
        _provider2.GetStatusAsync(flightNumber, date).Throws(new Exception("QuickFlight crash"));

        // Act
        var result = await _sut.ExecuteLookupAsync(flightNumber, date);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("SystemFallback", result.DataSource);
        Assert.Equal(UnifiedFlightStatus.Unknown, result.Status);
        Assert.Contains("unavailable", result.DelayReason);
    }

    [Fact]
    public async Task ExecuteLookupAsync_WhenFlightNumberIsEmpty_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.ExecuteLookupAsync("", new DateOnly(2026, 7, 6)));
    }
}
