using Application.Auth.UseCases.ForgotPassword;
using Domain.Shared.ValueObjects;
using Presentation.Shared.Extensions;

namespace Presentation.Auth.Endpoints;

public sealed record ForgotPasswordRequest(string Email);

public sealed class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email).ValidateValueObject(Email.Create);
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

                    var command = request.ToCommand();
                    var result = await commandHandler.Handle(command, ct);

                    return result.IsFailure
                        ? result.Error.ToApiError(logger)
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

    private static ForgotPasswordCommand ToCommand(this ForgotPasswordRequest request)
    {
        var email = Email.Create(request.Email).Value;

        return new ForgotPasswordCommand(email);
    }
}
