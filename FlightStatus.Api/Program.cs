using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FluentValidation;
using FlightStatus.Api.Domain.Models;
using FlightStatus.Api.Dtos;
using FlightStatus.Api.Middleware;
using FlightStatus.Api.Providers;
using FlightStatus.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDevPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Automatically scan assembly and register FluentValidation validator classes
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Register supplier strategy implementations in DI container
builder.Services.AddScoped<IFlightStatusProvider, AeroTrackFlightStatusProvider>();
builder.Services.AddScoped<IFlightStatusProvider, QuickFlightFlightStatusProvider>();

builder.Services.AddScoped<IFlightStatusService, FlightStatusService>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AngularDevPolicy");

/// <summary>
/// GET /flights/status
/// Resolves the aggregated, normalized status of a given flight.
/// </summary>
/// <remarks>
/// Validates request parameters via <see cref="ValidationFilter{T}"/> before execution.
/// </remarks>
app.MapGet("/flights/status", async (
    [AsParameters] FlightStatusRequest request,
    [FromServices] IFlightStatusService flightStatusService) =>
{
    var date = DateOnly.ParseExact(request.DateStr!, "yyyy-MM-dd");
    var statusResult = await flightStatusService.ExecuteLookupAsync(request.FlightNumber!, date);

    return Results.Ok(statusResult);
})
.AddEndpointFilter<ValidationFilter<FlightStatusRequest>>()
.WithName("GetFlightStatus")
.WithSummary("Query and normalize flight status.")
.WithDescription("Queries registered flight providers concurrently, normalizes responses, and returns the latest status.")
.Produces<FlightStatusResult>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status500InternalServerError);

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.Run();
