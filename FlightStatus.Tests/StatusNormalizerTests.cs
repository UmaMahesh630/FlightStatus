using FlightStatus.Api.Domain.Enums;
using FlightStatus.Api.Services;
using Xunit;

namespace FlightStatus.Tests;

public class StatusNormalizerTests
{
    [Theory]
    [InlineData("ON_TIME", UnifiedFlightStatus.OnTime)]
    [InlineData("LATE", UnifiedFlightStatus.Delayed)]
    [InlineData("CANCELLED", UnifiedFlightStatus.Cancelled)]
    [InlineData("DIVERTED", UnifiedFlightStatus.Diverted)]
    [InlineData("UNKNOWN_CODE", UnifiedFlightStatus.Unknown)]
    [InlineData("", UnifiedFlightStatus.Unknown)]
    [InlineData(null, UnifiedFlightStatus.Unknown)]
    [InlineData("  on_time  ", UnifiedFlightStatus.OnTime)] // Case-insensitive and trimmed check
    public void MapAeroTrackStatus_ShouldNormalizeCorrectly(string? rawStatus, UnifiedFlightStatus expected)
    {
        // Act
        var result = StatusNormalizer.MapAeroTrackStatus(rawStatus);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("ON_SCHEDULE", UnifiedFlightStatus.OnTime)]
    [InlineData("DELAYED", UnifiedFlightStatus.Delayed)]
    [InlineData("CANCELED", UnifiedFlightStatus.Cancelled)]
    [InlineData("REROUTED", UnifiedFlightStatus.Diverted)]
    [InlineData("XYZ", UnifiedFlightStatus.Unknown)]
    [InlineData("", UnifiedFlightStatus.Unknown)]
    [InlineData(null, UnifiedFlightStatus.Unknown)]
    [InlineData("  delayed  ", UnifiedFlightStatus.Delayed)] // Case-insensitive and trimmed check
    public void MapQuickFlightStatus_ShouldNormalizeCorrectly(string? rawStatus, UnifiedFlightStatus expected)
    {
        // Act
        var result = StatusNormalizer.MapQuickFlightStatus(rawStatus);

        // Assert
        Assert.Equal(expected, result);
    }
}
