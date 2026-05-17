namespace Presentation.Shared;

public static class ErrorHandler
{
    public static IResult Handle(Error error, ILogger logger)
    {
        var title = "Error";
        var detail = error.Description;
        Dictionary<string, IEnumerable<string>>? errors = null;

        var statusCode = error.ErrorType switch
        {
            ErrorType.Validation or ErrorType.Failure => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.AccessForbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                "Error occurred. Type: {Type},  Description: {Description}",
                error.ErrorType,
                error.Description
            );

            title = "Internal";
            detail = "Internal server error";
        }

        if (error.ErrorType is ErrorType.Validation && error.Errors is not null)
            errors = new Dictionary<string, IEnumerable<string>>
            {
                { error.Errors.Value.Item1, error.Errors.Value.Item2 },
            };

        ProblemDetails problemDetails = new()
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
        };

        if (errors is not null)
            problemDetails.Extensions.TryAdd("errors", errors);

        return TypedResults.Problem(problemDetails);
    }
}
