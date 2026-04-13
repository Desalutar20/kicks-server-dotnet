using Application.Auth.Types;
using Application.Auth.UseCases.VerifyAccount;
using Application.Config;
using Presentation.Auth.Dto;

namespace Presentation.Auth.Endpoints;

public sealed record VerifyAccountRequest(string Token, string Email);

public sealed class VerifyAccountRequestValidator : AbstractValidator<VerifyAccountRequest>
{
    public VerifyAccountRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(Email.MaxLength);
    }
}

internal static partial class AuthEndpoint
{
    private static IEndpointRouteBuilder VerifyAccountV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("/verify-account",
                    async (
                        HttpContext ctx,
                        VerifyAccountRequest request,
                        ICommandHandler<VerifyAccountCommand, UserWithSessionId> commandHandler,
                        Config config,
                        ILoggerFactory loggerFactory,
                        CancellationToken ct = default) =>
                    {
                        var logger = loggerFactory.CreateLogger("Auth.VerifyAccount");

                        var command = request.ToCommand();
                        if (command.IsFailure)
                        {
                            return ErrorHandler.Handle(command.Error, logger);
                        }

                        var result = await commandHandler.Handle(command.Value, ct);
                        if (result.IsFailure)
                        {
                            return ErrorHandler.Handle(result.Error, logger);
                        }

                        var (user, sessionId) = result.Value;
                        if (sessionId is not null)
                        {
                            SetSessionCookie(ctx, sessionId.Value, config.Application);
                        }


                        return Results.Ok(new ApiResponse<UserDto>(user.ToDto()));
                    })
                .AddEndpointFilter<ValidationFilter>()
                .Produces<ApiResponse<UserDto>>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status500InternalServerError)
                .ProducesValidationProblem()
                .WithName("VerifyAccount")
                .WithSummary("Verifies a user's account")
                .WithDescription("Marks a user account as verified using the provided token and email.")
                .RequireRateLimiting(RateLimitConstants.VerifyAccount);

        return endpoint;
    }

    private static Result<VerifyAccountCommand> ToCommand(this VerifyAccountRequest request)
    {
        var token = NonEmptyString.Create(request.Token);
        if (token.IsFailure)
        {
            return Result<VerifyAccountCommand>.Failure(token.Error);
        }

        var email = Email.Create(request.Email);
        if (email.IsFailure)
        {
            return Result<VerifyAccountCommand>.Failure(email.Error);
        }

        return Result<VerifyAccountCommand>.Success(
            new VerifyAccountCommand(token.Value, email.Value));
    }
}