using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FlightStatus.Api.Middleware;

/// <summary>
/// Modern global exception handler implementing .NET 8's <see cref="IExceptionHandler"/> interface.
/// </summary>
/// <remarks>
/// ARCHITECTURE & DESIGN DECISIONS:
/// - **Modern .NET 8 Exception Handling**: Replaces traditional custom middleware with .NET 8's native, 
///   high-performance exception handling system.
/// - **RFC 7807 Problem Details**: Generates standard, machine-readable HTTP API error responses (`ProblemDetails`), 
///   which is the industry standard for RESTful error communication.
/// - **Structured Logging**: Captures unhandled exceptions with deep context, formatting log messages structured 
///   for search engines/APMs (e.g. Elasticsearch or Application Insights).
/// - **SOLID - Single Responsibility Principle (SRP)**: This handler is solely responsible for catching, logging, 
///   and translating unhandled system exceptions into clean HTTP responses, separating error responses from core API endpoints.
/// </remarks>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. Structured Logging of the Exception
        _logger.LogError(
            exception, 
            "An unhandled exception occurred while processing request on path {Path}. Reason: {ErrorMessage}", 
            httpContext.Request.Path, 
            exception.Message);

        // 2. Build standard RFC 7807 ProblemDetails response
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected server error occurred",
            Detail = exception.Message, // In production systems, you would hide the exception details for security
            Instance = httpContext.Request.Path,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };

        // Add trace identifier for tracking/debugging requests
        problemDetails.Extensions.Add("traceId", httpContext.TraceIdentifier);

        // 3. Send response
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // Return true to signal that this exception is handled and pipeline execution should stop
        return true;
    }
}
