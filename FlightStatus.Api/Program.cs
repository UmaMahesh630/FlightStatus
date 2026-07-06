using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FlightStatus.Api.Domain.Models;
using FlightStatus.Api.Middleware;
using FlightStatus.Api.Providers;
using FlightStatus.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// =========================================================================
// DEPENDENCY INJECTION & CONFIGURATION REGISTRATION
// =========================================================================

// 1. Configure CORS (Cross-Origin Resource Sharing)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDevPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 2. Global Exception Handling and Problem Details (.NET 8 standard):
//    DESIGN RATIONALE: Registers our custom global IExceptionHandler and enabling built-in support for 
//    ProblemDetails (RFC 7807) to return standardized machine-readable HTTP error structures.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// 3. Providers (Strategy Pattern):
builder.Services.AddScoped<IFlightStatusProvider, AeroTrackFlightStatusProvider>();
builder.Services.AddScoped<IFlightStatusProvider, QuickFlightFlightStatusProvider>();

// 4. Orchestration Service:
builder.Services.AddScoped<IFlightStatusService, FlightStatusService>();

// =========================================================================

var app = builder.Build();

// Configure the HTTP request pipeline.

// Apply Global Exception Handler middleware early in the pipeline to catch downstream exceptions.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    // Swagger/OpenAPI configuration can go here
}

app.UseHttpsRedirection();

// Apply CORS Policy
app.UseCors("AngularDevPolicy");

// =========================================================================
// MINIMAL API ENDPOINTS
// =========================================================================

/// <summary>
/// GET /flights/status
/// Retrieves normalized flight status information for a given flight number and date.
/// </summary>
app.MapGet("/flights/status", async (
    [FromQuery(Name = "flightNumber")] string? flightNumber,
    [FromQuery(Name = "date")] string? dateStr,
    [FromServices] IFlightStatusService flightStatusService) =>
{
    // 1. Input Validation (Defense in Depth)
    if (string.IsNullOrWhiteSpace(flightNumber))
    {
        return Results.BadRequest(new { error = "flightNumber query parameter is required." });
    }

    if (string.IsNullOrWhiteSpace(dateStr))
    {
        return Results.BadRequest(new { error = "date query parameter is required." });
    }

    if (!DateOnly.TryParseExact(dateStr, "yyyy-MM-dd", out var date))
    {
        return Results.BadRequest(new { error = "date parameter must be in 'yyyy-MM-dd' format." });
    }

    // 2. Execute Orchestration Logic
    var statusResult = await flightStatusService.ExecuteLookupAsync(flightNumber, date);

    // 3. Return Mapped Unified Result
    return Results.Ok(statusResult);
})
.WithName("GetFlightStatus")
.WithSummary("Query and normalize flight status.")
.WithDescription("Queries registered flight providers concurrently, normalizes responses, and returns the latest status.")
.Produces<FlightStatusResult>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status500InternalServerError);

// Basic health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.Run();
