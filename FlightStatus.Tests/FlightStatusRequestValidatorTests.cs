using FlightStatus.Api.Dtos;
using FlightStatus.Api.Validators;
using Xunit;

namespace FlightStatus.Tests;

public class FlightStatusRequestValidatorTests
{
    private readonly FlightStatusRequestValidator _validator;

    public FlightStatusRequestValidatorTests()
    {
        _validator = new FlightStatusRequestValidator();
    }

    [Theory]
    [InlineData("AI101", "2026-07-06")]
    [InlineData("BA202", "2026-12-31")]
    [InlineData("UA303", "2026-01-01")]
    public void Validator_WithValidRequest_ShouldPass(string flightNumber, string dateStr)
    {
        // Arrange
        var request = new FlightStatusRequest
        {
            FlightNumber = flightNumber,
            DateStr = dateStr
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void Validator_WithMissingFlightNumber_ShouldFail(string? flightNumber)
    {
        // Arrange
        var request = new FlightStatusRequest
        {
            FlightNumber = flightNumber,
            DateStr = "2026-07-06"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal("FlightNumber", error.PropertyName);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("AB")]
    [InlineData("ABC123456789")] // Over 10 characters
    [InlineData("AI-101")] // Symbol not allowed
    public void Validator_WithInvalidFlightNumberFormat_ShouldFail(string flightNumber)
    {
        // Arrange
        var request = new FlightStatusRequest
        {
            FlightNumber = flightNumber,
            DateStr = "2026-07-06"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FlightNumber");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void Validator_WithMissingDate_ShouldFail(string? dateStr)
    {
        // Arrange
        var request = new FlightStatusRequest
        {
            FlightNumber = "AI101",
            DateStr = dateStr
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "DateStr");
    }

    [Theory]
    [InlineData("2026/07/06")] // Slash separators
    [InlineData("06-07-2026")] // DD-MM-YYYY format
    [InlineData("2026-7-6")]    // Single digit Month/Day
    [InlineData("InvalidDate")]  // Alphabetic string
    public void Validator_WithInvalidDateFormat_ShouldFail(string dateStr)
    {
        // Arrange
        var request = new FlightStatusRequest
        {
            FlightNumber = "AI101",
            DateStr = dateStr
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "DateStr" && e.ErrorMessage.Contains("format"));
    }

    [Theory]
    [InlineData("2026-02-30")] // February 30th (does not exist)
    [InlineData("2026-04-31")] // April 31st (does not exist)
    [InlineData("2026-13-01")] // 13th month
    [InlineData("2026-06-32")] // 32nd day
    public void Validator_WithNonExistentCalendarDate_ShouldFail(string dateStr)
    {
        // Arrange
        var request = new FlightStatusRequest
        {
            FlightNumber = "AI101",
            DateStr = dateStr
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "DateStr" && e.ErrorMessage.Contains("valid calendar"));
    }
}
