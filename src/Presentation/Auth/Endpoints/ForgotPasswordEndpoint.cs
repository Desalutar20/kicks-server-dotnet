using Application.Auth.UseCases.ForgotPassword;

namespace Presentation.Auth.Endpoints;

public sealed record ForgotPasswordRequest(string Email);

public sealed class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(Email.MaxLength);
    }
}

internal static partial class AuthEndpoint
{
    private static IEndpointRouteBuilder ForgotPasswordV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost(
                "/forgot-password",
                async (
                    ForgotPasswordRequest request,
                    ICommandHandler<ForgotPasswordCommand> commandHandler,
                    ILoggerFactory loggerFactory,
                    CancellationToken ct = default
                ) =>
                {
                    var logger = loggerFactory.CreateLogger("Auth.ForgotPassword");

                    var commandResult = request.ToCommand();
                    if (commandResult.IsFailure)
                    {
                        return ErrorHandler.Handle(commandResult.Error, logger);
                    }

                    var result = await commandHandler.Handle(commandResult.Value, ct);
                    return result.IsFailure
                        ? ErrorHandler.Handle(result.Error, logger)
                        : Results.Ok(
                            new ApiResponse<string>(
                                "If the email is registered, password reset instructions have been sent"
                            )
                        );
                }
            )
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<string>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("ForgotPassword")
            .WithSummary("Sends a password reset email to the user.")
            .WithDescription("Triggers the password reset workflow using the provided email.")
            .RequireRateLimiting(RateLimitConstants.ForgotPassword);

        return endpoint;
    }

    private static Result<ForgotPasswordCommand> ToCommand(this ForgotPasswordRequest request)
    {
        var email = Email.Create(request.Email);
        if (email.IsFailure)
        {
            return Result<ForgotPasswordCommand>.Failure(email.Error);
        }

        return Result<ForgotPasswordCommand>.Success(new ForgotPasswordCommand(email.Value));
    }
}
