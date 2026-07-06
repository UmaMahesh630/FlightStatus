using FlightStatus.Api.Providers;
using FlightStatus.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// =========================================================================
// DEPENDENCY INJECTION REGISTRATION
// =========================================================================

// 1. Providers (Strategy Pattern):
//    We register both concrete implementations of IFlightStatusProvider. The .NET DI container automatically 
//    groups all registered types under IEnumerable<IFlightStatusProvider> which is then injected into our service.
//    Lifetime: Scoped (created once per HTTP request, fitting standard API client/database connections).
builder.Services.AddScoped<IFlightStatusProvider, AeroTrackFlightStatusProvider>();
builder.Services.AddScoped<IFlightStatusProvider, QuickFlightFlightStatusProvider>();

// 2. Orchestration Service:
//    Lifetime: Scoped.
builder.Services.AddScoped<IFlightStatusService, FlightStatusService>();

// 3. Mapping Layer:
//    DESIGN NOTE: Mapping raw vendor DTOs into the internal domain model is implemented via stateless, pure 
//    static functions (in StatusNormalizer and within provider mapping methods). This eliminates the need to 
//    register separate Mapper classes in DI, avoiding object allocation overhead and DI container bloat.
// =========================================================================

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Swagger/OpenAPI configuration can go here
}

app.UseHttpsRedirection();

// Temporary endpoint for validation
app.MapGet("/health", () => Results.Ok("Flight Status API is up and running."));

app.Run();
