using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FluentValidation;
using FlightStatus.Api.Domain.Models;
using FlightStatus.Api.Dtos;
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

// 2. Global Exception Handling and Problem Details (.NET 8 standard)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// 3. Register FluentValidation Validators:
//    DESIGN DECISION: Registers all validators (like FlightStatusRequestValidator) automatically from 
//    the assembly, reducing registration maintenance effort as more validators are created.
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// 4. Providers (Strategy Pattern):
builder.Services.AddScoped<IFlightStatusProvider, AeroTrackFlightStatusProvider>();
builder.Services.AddScoped<IFlightStatusProvider, QuickFlightFlightStatusProvider>();

// 5. Orchestration Service:
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
/// <remarks>
/// ARCHITECTURE & DESIGN DECISIONS:
/// - **Parameter Bundling ([AsParameters])**: Captures all incoming query arguments into a cohesive DTO.
/// - **Cross-Cutting Validation (AOP Filter)**: Appends AddEndpointFilter for validation. The endpoint code 
///   does not execute manual checks; instead, ValidationFilter interceptors check both Data Annotations 
///   and FluentValidation rules before execution, conforming to the Single Responsibility Principle.
/// </remarks>
app.MapGet("/flights/status", async (
    [AsParameters] FlightStatusRequest request,
    [FromServices] IFlightStatusService flightStatusService) =>
{
    // Parse the date (guaranteed valid due to validation filter checks)
    var date = DateOnly.ParseExact(request.DateStr, "yyyy-MM-dd");

    // Execute Orchestration Logic
    var statusResult = await flightStatusService.ExecuteLookupAsync(request.FlightNumber, date);

    return Results.Ok(statusResult);
})
.AddEndpointFilter<ValidationFilter<FlightStatusRequest>>()
.WithName("GetFlightStatus")
.WithSummary("Query and normalize flight status.")
.WithDescription("Queries registered flight providers concurrently, normalizes responses, and returns the latest status.")
.Produces<FlightStatusResult>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status500InternalServerError);

// Basic health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.Run();
