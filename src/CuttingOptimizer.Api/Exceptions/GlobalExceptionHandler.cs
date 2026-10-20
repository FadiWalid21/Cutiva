using CuttingOptimizer.Application.Resources;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace CuttingOptimizer.Api.Exceptions;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IStringLocalizer<Messages> localizer)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        var baseError = exception.GetBaseException().Message;

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "An unexpected error occurred at {Path}: {Error}",
                httpContext.Request.Path.Value,
                baseError);
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode == StatusCodes.Status400BadRequest
                ? localizer["ValidationFailed"].Value
                : localizer["UnexpectedError"].Value,
            Detail = statusCode == StatusCodes.Status400BadRequest
                ? localizer["ValidationFailedDetail"].Value
                : null,
            Instance = httpContext.Request.Path
        };

        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(error =>
                    string.IsNullOrWhiteSpace(error.PropertyName)
                        ? "form"
                        : char.ToLowerInvariant(error.PropertyName[0])
                          + error.PropertyName[1..])
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray());

            problem.Extensions["errors"] = errors;
            problem.Extensions["message"] = localizer["ValidationFailedDetail"].Value;
        }
        else if (exception is DbUpdateException || exception.GetBaseException() is DbUpdateException)
        {
            // Surface store errors for workshop ops (e.g. schema drift) instead of an opaque 500.
            problem.Extensions["message"] = baseError;
        }

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problem,
            cancellationToken);

        return true;
    }
}
