using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FlightStatus.Api.Domain.Models;
using FlightStatus.Api.Providers;
using FlightStatus.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// =========================================================================
// DEPENDENCY INJECTION & CONFIGURATION REGISTRATION
// =========================================================================

// 1. Configure CORS (Cross-Origin Resource Sharing):
//    DESIGN RATIONALE: The Angular frontend will run on http://localhost:4200, which is a different origin. 
//    Enabling a specific CORS policy prevents browser CORS blocks without exposing the API to wildcard risks.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDevPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 2. Providers (Strategy Pattern):
//    We register both concrete implementations of IFlightStatusProvider. The .NET DI container automatically 
//    groups all registered types under IEnumerable<IFlightStatusProvider> which is then injected into our service.
builder.Services.AddScoped<IFlightStatusProvider, AeroTrackFlightStatusProvider>();
builder.Services.AddScoped<IFlightStatusProvider, QuickFlightFlightStatusProvider>();

// 3. Orchestration Service:
builder.Services.AddScoped<IFlightStatusService, FlightStatusService>();

// =========================================================================

var app = builder.Build();

// Configure the HTTP request pipeline.
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
/// <remarks>
/// ARCHITECTURE & DESIGN DECISIONS:
/// - **Minimal API Endpoint**: Lightweight routing construct that reduces middleware overhead compared to full MVC controllers.
/// - **SOLID - Interface Boundary**: Only interacts with IFlightStatusService, satisfying the Dependency Inversion Principle.
/// - **Defensive Input Validation**: Validates arguments before calling the service. If flightNumber is missing or 
///   the date format is invalid, returns 400 Bad Request directly to prevent downstream processing errors.
/// - **OpenAPI Documentation**: Enriched with Produces and summary metadata to support self-documenting APIs.
/// </remarks>
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
