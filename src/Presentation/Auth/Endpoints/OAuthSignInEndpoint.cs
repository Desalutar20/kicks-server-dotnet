using Application.Abstractions.OAuth;
using Application.Auth.UseCases.OAuthSignIn;
using Application.Config;

namespace Presentation.Auth.Endpoints;

internal static partial class AuthEndpoint
{
    private static IEndpointRouteBuilder OAuthSignInV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("/{provider}/callback",
                    async (
                        HttpContext ctx,
                        Config config,
                        ILoggerFactory loggerFactory,
                        ICommandHandler<OAuthSignInCommand, Guid> commandHandler,
                        [FromRoute] string provider,
                        [FromQuery] string code,
                        [FromQuery] string state,
                        CancellationToken ct = default) =>
                    {
                        var oAuthStateCookieName = config.Application.OAuthStateCookieName;
                        if (!ctx.Request.Cookies.TryGetValue(oAuthStateCookieName,
                                out var expectedState))
                        {
                            return TypedResults.Problem(
                                "Invalid state",
                                statusCode: StatusCodes.Status400BadRequest
                            );
                        }

                        var logger = loggerFactory.CreateLogger("Auth.OAuthSignIn");

                        var commandResult = ToCommand(state, code, provider, expectedState);
                        if (commandResult.IsFailure)
                        {
                            return ErrorHandler.Handle(commandResult.Error, logger);
                        }

                        var result = await commandHandler.Handle(commandResult.Value, ct);
                        if (result.IsFailure)
                        {
                            return ErrorHandler.Handle(result.Error, logger);
                        }

                        SetSessionCookie(ctx, result.Value, config.Application);

                        return Results.Ok(new ApiResponse<string>("success"));
                    })
                .AddEndpointFilter<ValidationFilter>()
                .Produces<ApiResponse<string>>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesValidationProblem()
                .ProducesProblem(StatusCodes.Status500InternalServerError)
                .ProducesValidationProblem()
                .WithName("OAuthSignIn")
                .WithSummary("OAuth sign-in callback")
                .WithDescription("Handles OAuth provider callback and completes user authentication flow.")
                .RequireRateLimiting(RateLimitConstants.SignIn);

        return endpoint;
    }

    private static Result<OAuthSignInCommand> ToCommand(string state, string code, string provider,
        string expectedState)
    {
        if (!Enum.TryParse<OAuthProvider>(provider, true, out var parsedProvider))
        {
            return Result<OAuthSignInCommand>.Failure(
                Error.Validation("provider", ["Invalid oauth provider"]));
        }

        var codeResult = NonEmptyString.Create(code);
        if (codeResult.IsFailure)
        {
            return Result<OAuthSignInCommand>.Failure(codeResult.Error);
        }

        var receivedStateResult = OAuthState.Create(state);
        if (receivedStateResult.IsFailure)
        {
            return Result<OAuthSignInCommand>.Failure(receivedStateResult.Error);
        }

        var expectedStateResult = OAuthState.Create(expectedState);
        if (expectedStateResult.IsFailure)
        {
            return Result<OAuthSignInCommand>.Failure(expectedStateResult.Error);
        }

        return Result<OAuthSignInCommand>.Success(
            new OAuthSignInCommand(parsedProvider, codeResult.Value, receivedStateResult.Value,
                expectedStateResult.Value));
    }
}