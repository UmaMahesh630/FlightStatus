using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FlightStatus.Api.Middleware;

/// <summary>
/// A reusable, generic endpoint filter that runs both Data Annotations and FluentValidation.
/// </summary>
/// <remarks>
/// ARCHITECTURE & DESIGN DECISIONS:
/// - **ASP.NET Core Endpoint Filters**: Introduced in .NET 7/8, these filters enable Aspect-Oriented Programming (AOP). 
///   By executing validation as a cross-cutting concern, we keep endpoint route handlers clean and focused only 
///   on executing business orchestration.
/// - **Dual Validation Engine**: Performs both standard C# Data Annotations validation (via Validator) and 
///   FluentValidation (via DI resolved IValidator). This demonstrates how declarative metadata validation and 
///   logical fluent validation can co-exist.
/// - **Standardized Validation Details**: Returns a <see cref="Microsoft.AspNetCore.Http.HttpResults.ValidationProblem"/> 
///   response which structures errors inside a standard ProblemDetails (RFC 7807) payload.
/// </remarks>
public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Find the argument matching the validated type
        var argToValidate = context.Arguments.FirstOrDefault(x => x is T) as T;
        if (argToValidate == null)
        {
            return await next(context);
        }

        // 1. Data Annotations Validation
        var validationContext = new ValidationContext(argToValidate);
        var validationResults = new List<ValidationResult>();
        bool isValidAnnotations = Validator.TryValidateObject(argToValidate, validationContext, validationResults, true);

        if (!isValidAnnotations)
        {
            var errors = validationResults
                .GroupBy(vr => vr.MemberNames.FirstOrDefault() ?? "Request")
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(vr => vr.ErrorMessage ?? "Invalid value").ToArray()
                );

            return Results.ValidationProblem(errors);
        }

        // 2. Fluent Validation (if registered in DI)
        var fluentValidator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (fluentValidator != null)
        {
            var fluentResult = await fluentValidator.ValidateAsync(argToValidate);
            if (!fluentResult.IsValid)
            {
                var errors = fluentResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                return Results.ValidationProblem(errors);
            }
        }

        return await next(context);
    }
}
