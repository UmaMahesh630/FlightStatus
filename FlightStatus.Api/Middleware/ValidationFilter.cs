using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FlightStatus.Api.Middleware;

/// <summary>
/// A generic endpoint filter executing both Data Annotations and FluentValidation.
/// </summary>
/// <remarks>
/// Intercepts incoming parameters in the Minimal API pipeline (Aspect-Oriented validation). 
/// Executes standard Data Annotations, followed by FluentValidation rules if an <see cref="IValidator{T}"/> 
/// is registered in DI, returning a RFC 7807 ValidationProblem response if validation fails.
/// </remarks>
public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var argToValidate = context.Arguments.FirstOrDefault(x => x is T) as T;
        if (argToValidate == null)
        {
            return await next(context);
        }

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
