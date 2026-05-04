using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.ExceptionHandler;

internal class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var (title, detail, status, errors) = exception switch
        {
            ValidationException ex => (
                "Validation failed",
                "Validation failed",
                HttpStatusCode.BadRequest,
                ex.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key.ToLower(),
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    )
            ),
            _ => (
                "An error occurred",
                "Something went wrong",
                HttpStatusCode.InternalServerError,
                null
            ),
        };

        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = (int)status;

        if (status == HttpStatusCode.InternalServerError)
            logger.LogError(
                exception,
                "Unhandled exception occurred: {Message}",
                exception.Message
            );

        ProblemDetails problemDetails = new()
        {
            Title = title,
            Status = (int)status,
            Detail = detail,
        };

        if (errors is not null)
            problemDetails.Extensions.TryAdd("errors", errors);

        var result = await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = problemDetails }
        );

        if (!result)
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
