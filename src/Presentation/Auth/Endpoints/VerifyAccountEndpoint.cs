using Application.Auth.UseCases.VerifyAccount;
using Domain.Shared.ValueObjects;
using Presentation.Shared.Extensions;

namespace Presentation.Auth.Endpoints;

public sealed record VerifyAccountRequest(string Token, string Email);

public sealed class VerifyAccountRequestValidator : AbstractValidator<VerifyAccountRequest>
{
    public VerifyAccountRequestValidator()
    {
        RuleFor(x => x.Token)
            .ValidateValueObject(x => NonEmptyString.Create(x, "Token"))
            .MaximumLength(100);
        RuleFor(x => x.Email).ValidateValueObject(Email.Create);
    }
}

internal static partial class AuthEndpoint
{
    private static IEndpointRouteBuilder VerifyAccountV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost(
                "/verify-account",
                async (
                    VerifyAccountRequest request,
                    ICommandHandler<VerifyAccountCommand> commandHandler,
                    ILoggerFactory loggerFactory,
                    CancellationToken ct = default
                ) =>
                {
                    var logger = loggerFactory.CreateLogger("Auth.VerifyAccount");

                    var command = request.ToCommand();
                    var result = await commandHandler.Handle(command, ct);

                    return result.IsFailure
                        ? result.Error.ToApiError(logger)
                        : Results.Ok(
                            new ApiResponse<string>(
                                "Your account has been successfully verified. You can now log in."
                            )
                        );
                }
            )
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<string>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("VerifyAccount")
            .WithSummary("Verifies a user's account")
            .WithDescription("Marks a user account as verified using the provided token and email.")
            .RequireRateLimiting(RateLimitConstants.VerifyAccount);

        return endpoint;
    }

    private static VerifyAccountCommand ToCommand(this VerifyAccountRequest request)
    {
        var token = NonEmptyString.Create(request.Token).Value;
        var email = Email.Create(request.Email).Value;

        return new VerifyAccountCommand(token, email);
    }
}
