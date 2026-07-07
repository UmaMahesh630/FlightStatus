using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FlightStatus.Api.Domain.Enums;
using FlightStatus.Api.Domain.Models;
using FlightStatus.Api.Providers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace FlightStatus.Tests;

public class FlightStatusIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public FlightStatusIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetStatus_WithValidRequest_ReturnsHttp200AndCorrectResult()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/flights/status?flightNumber=AI101&date=2026-07-07");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<FlightStatusResult>(_jsonSerializerOptions);
        Assert.NotNull(result);
        Assert.Equal("AI101", result.FlightNumber);
        Assert.Equal(UnifiedFlightStatus.OnTime, result.Status);
    }

    [Fact]
    public async Task GetStatus_MissingFlightNumber_ReturnsHttp400()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/flights/status?date=2026-07-07");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetStatus_MissingDate_ReturnsHttp400()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/flights/status?flightNumber=AI101");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("hjgjhgj")]
    [InlineData("abcdef")]
    [InlineData("test123")]
    [InlineData("invalid")]
    public async Task GetStatus_InvalidFlightNumberPattern_ReturnsHttp400WithValidationMessage(string flightNumber)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/flights/status?flightNumber={flightNumber}&date=2026-07-07");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problemDetails = await response.Content.ReadAsStringAsync();
        Assert.Contains("Flight number must consist of 2 to 3 uppercase letters followed by 1 to 4 digits", problemDetails);
    }

    [Fact]
    public async Task GetStatus_UnknownFlight_ReturnsHttp200WithUnknownStatus()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/flights/status?flightNumber=XY999&date=2026-07-07");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<FlightStatusResult>(_jsonSerializerOptions);
        Assert.NotNull(result);
        Assert.Equal(UnifiedFlightStatus.Unknown, result.Status);
        Assert.Equal("SystemFallback", result.DataSource);
    }

    [Fact]
    public async Task GetStatus_WhenBothProvidersReturnData_SelectsLatestLastUpdatedUtc()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act & Assert 1: For AI101, QuickFlight is newer (11:15 AM vs 11:00 AM)
        var response1 = await client.GetAsync("/flights/status?flightNumber=AI101&date=2026-07-07");
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        var result1 = await response1.Content.ReadFromJsonAsync<FlightStatusResult>(_jsonSerializerOptions);
        Assert.NotNull(result1);
        Assert.Equal("QuickFlight", result1.DataSource);

        // Act & Assert 2: For BA202, AeroTrack is newer (3:45 PM vs 3:30 PM)
        var response2 = await client.GetAsync("/flights/status?flightNumber=BA202&date=2026-07-07");
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        var result2 = await response2.Content.ReadFromJsonAsync<FlightStatusResult>(_jsonSerializerOptions);
        Assert.NotNull(result2);
        Assert.Equal("AeroTrack", result2.DataSource);
    }

    [Fact]
    public async Task GetStatus_WhenOneProviderFails_ReturnsOtherProviderResponse()
    {
        // Arrange
        var mockFailingProvider = Substitute.For<IFlightStatusProvider>();
        mockFailingProvider.ProviderName.Returns("FailingProvider");
        mockFailingProvider
            .GetStatusAsync(Arg.Any<string>(), Arg.Any<DateOnly>())
            .Returns(Task.FromException<FlightStatusResult?>(new HttpRequestException("Simulated provider connection failure")));

        var mockWorkingProvider = Substitute.For<IFlightStatusProvider>();
        mockWorkingProvider.ProviderName.Returns("WorkingProvider");
        var expectedResult = new FlightStatusResult
        {
            FlightNumber = "AI101",
            Date = new DateOnly(2026, 7, 7),
            Status = UnifiedFlightStatus.OnTime,
            ScheduledDeparture = DateTime.UtcNow,
            ScheduledArrival = DateTime.UtcNow,
            DataSource = "WorkingProvider",
            LastUpdatedUtc = DateTime.UtcNow
        };
        mockWorkingProvider
            .GetStatusAsync(Arg.Any<string>(), Arg.Any<DateOnly>())
            .Returns(expectedResult);

        var customFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Remove existing providers and replace with mock configurations
                var descriptors = services.Where(d => d.ServiceType == typeof(IFlightStatusProvider)).ToList();
                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }
                services.AddScoped(_ => mockFailingProvider);
                services.AddScoped(_ => mockWorkingProvider);
            });
        });

        var client = customFactory.CreateClient();

        // Act
        var response = await client.GetAsync("/flights/status?flightNumber=AI101&date=2026-07-07");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<FlightStatusResult>(_jsonSerializerOptions);
        Assert.NotNull(result);
        Assert.Equal("WorkingProvider", result.DataSource);
        Assert.Equal(UnifiedFlightStatus.OnTime, result.Status);
    }

    [Fact]
    public async Task GetStatus_WhenAllProvidersFail_ReturnsUnknownWithConfiguredMessage()
    {
        // Arrange
        var mockFailingProvider1 = Substitute.For<IFlightStatusProvider>();
        mockFailingProvider1.ProviderName.Returns("FailingProvider1");
        mockFailingProvider1
            .GetStatusAsync(Arg.Any<string>(), Arg.Any<DateOnly>())
            .Returns(Task.FromException<FlightStatusResult?>(new HttpRequestException("Simulated network timeout")));

        var mockFailingProvider2 = Substitute.For<IFlightStatusProvider>();
        mockFailingProvider2.ProviderName.Returns("FailingProvider2");
        mockFailingProvider2
            .GetStatusAsync(Arg.Any<string>(), Arg.Any<DateOnly>())
            .Returns(Task.FromException<FlightStatusResult?>(new HttpRequestException("Supplier API down")));

        var customFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                var descriptors = services.Where(d => d.ServiceType == typeof(IFlightStatusProvider)).ToList();
                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }
                services.AddScoped(_ => mockFailingProvider1);
                services.AddScoped(_ => mockFailingProvider2);
            });
        });

        var client = customFactory.CreateClient();

        // Act
        var response = await client.GetAsync("/flights/status?flightNumber=AI101&date=2026-07-07");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<FlightStatusResult>(_jsonSerializerOptions);
        Assert.NotNull(result);
        Assert.Equal(UnifiedFlightStatus.Unknown, result.Status);
        Assert.Equal("SystemFallback", result.DataSource);
        Assert.Equal("Flight status currently unavailable from all providers.", result.DelayReason);
    }
}
