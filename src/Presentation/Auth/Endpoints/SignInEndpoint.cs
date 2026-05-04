using Application.Auth.Types;
using Application.Auth.UseCases.SignIn;
using Application.Config;
using Presentation.Auth.Dto;

namespace Presentation.Auth.Endpoints;

public sealed record SignInRequest(string Email, string Password);

public sealed class SignInRequestValidator : AbstractValidator<SignInRequest>
{
    public SignInRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(Email.MaxLength);
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(Password.MinLength)
            .MaximumLength(Password.MaxLength);
    }
}

internal static partial class AuthEndpoint
{
    private static IEndpointRouteBuilder SignInV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost(
                "/sign-in",
                async (
                    HttpContext ctx,
                    SignInRequest request,
                    ICommandHandler<SignInCommand, UserWithSessionId> commandHandler,
                    Config config,
                    ILoggerFactory loggerFactory,
                    CancellationToken ct = default
                ) =>
                {
                    var logger = loggerFactory.CreateLogger("Auth.SignIn");

                    var commandResult = request.ToCommand();
                    if (commandResult.IsFailure)
                    {
                        return ErrorHandler.Handle(commandResult.Error, logger);
                    }

                    var result = await commandHandler.Handle(commandResult.Value, ct);
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
                }
            )
            .AddEndpointFilter<ValidationFilter>()
            .Produces<ApiResponse<UserDto>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("SignIn")
            .WithSummary("Authenticates a user")
            .WithDescription(
                "Logs in a user using email and password and returns authentication info"
            )
            .RequireRateLimiting(RateLimitConstants.SignIn);

        return endpoint;
    }

    private static Result<SignInCommand> ToCommand(this SignInRequest request)
    {
        var email = Email.Create(request.Email);
        if (email.IsFailure)
        {
            return Result<SignInCommand>.Failure(email.Error);
        }

        var password = Password.Create(request.Password);
        if (password.IsFailure)
        {
            return Result<SignInCommand>.Failure(password.Error);
        }

        return Result<SignInCommand>.Success(new SignInCommand(email.Value, password.Value));
    }
}
