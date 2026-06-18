using Application.Auth.UseCases.ResetPassword;
using Domain.Shared.ValueObjects;
using Presentation.Shared.Extensions;

namespace Presentation.Auth.Endpoints;

public sealed record ResetPasswordRequest(string Token, string Email, string NewPassword);

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Token)
            .ValidateValueObject(x => NonEmptyString.Create(x, "Token"))
            .MaximumLength(100);
        RuleFor(x => x.Email).ValidateValueObject(Email.Create);
        RuleFor(x => x.NewPassword).ValidateValueObject(Password.Create);
    }
}

internal static partial class AuthEndpoint
{
    private static IEndpointRouteBuilder ResetPasswordV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost(
                "/reset-password",
                async (
                    ResetPasswordRequest request,
                    ICommandHandler<ResetPasswordCommand> commandHandler,
                    ILoggerFactory loggerFactory,
                    CancellationToken ct = default
                ) =>
                {
                    var logger = loggerFactory.CreateLogger("Auth.ResetPassword");

                    var command = request.ToCommand();
                    var result = await commandHandler.Handle(command, ct);

                    return result.IsFailure
                        ? result.Error.ToApiError(logger)
                        : Results.Ok(new ApiResponse<string>(""));
                }
            )
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<string>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("ResetPassword")
            .WithSummary("Resets a user's password using a valid token.")
            .WithDescription(
                "Allows a user to set a new password if they provide a valid reset token and their email."
            )
            .RequireRateLimiting(RateLimitConstants.ResetPassword);

        return endpoint;
    }

    private static ResetPasswordCommand ToCommand(this ResetPasswordRequest request)
    {
        var token = NonEmptyString.Create(request.Token).Value;
        var email = Email.Create(request.Email).Value;
        var newPassword = Password.Create(request.NewPassword).Value;

        return new ResetPasswordCommand(token, email, newPassword);
    }
}
