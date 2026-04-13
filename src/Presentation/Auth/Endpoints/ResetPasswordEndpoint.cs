using Application.Auth.UseCases.ResetPassword;

namespace Presentation.Auth.Endpoints;

public sealed record ResetPasswordRequest(string Token, string Email, string NewPassword);

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(Email.MaxLength);
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(Password.MinLength)
            .MaximumLength(Password.MaxLength);
    }
}

internal static partial class AuthEndpoint
{
    private static IEndpointRouteBuilder ResetPasswordV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("/reset-password",
                    async (
                        ResetPasswordRequest request,
                        ICommandHandler<ResetPasswordCommand> commandHandler,
                        ILoggerFactory loggerFactory,
                        CancellationToken ct = default) =>
                    {
                        var logger = loggerFactory.CreateLogger("Auth.ResetPassword");

                        var commandResult = request.ToCommand();
                        if (commandResult.IsFailure)
                        {
                            return ErrorHandler.Handle(commandResult.Error, logger);
                        }

                        var result = await commandHandler.Handle(commandResult.Value, ct);
                        return result.IsFailure
                            ? ErrorHandler.Handle(result.Error, logger)
                            : Results.Ok(new ApiResponse<string>(""));
                    })
                .AddEndpointFilter<ValidationFilter>()
                .Produces<ApiResponse<string>>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status500InternalServerError)
                .ProducesValidationProblem()
                .WithName("ResetPassword")
                .WithSummary("Resets a user's password using a valid token.")
                .WithDescription(
                    "Allows a user to set a new password if they provide a valid reset token and their email.")
                .RequireRateLimiting(RateLimitConstants.ResetPassword);

        return endpoint;
    }

    private static Result<ResetPasswordCommand> ToCommand(this ResetPasswordRequest request)
    {
        var token = NonEmptyString.Create(request.Token);
        if (token.IsFailure)
        {
            return Result<ResetPasswordCommand>.Failure(token.Error);
        }

        var email = Email.Create(request.Email);
        if (email.IsFailure)
        {
            return Result<ResetPasswordCommand>.Failure(email.Error);
        }

        var newPassword = Password.Create(request.NewPassword);
        if (newPassword.IsFailure)
        {
            return Result<ResetPasswordCommand>.Failure(
                newPassword.Error);
        }


        return Result<ResetPasswordCommand>.Success(
            new ResetPasswordCommand(token.Value, email.Value, newPassword.Value));
    }
}