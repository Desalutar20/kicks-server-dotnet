using Application.Abstractions.OAuth;
using Application.Auth.UseCases.GenerateOAuthUrl;
using Application.Config;
using Domain.Shared.ValueObjects;
using Presentation.Shared.Extensions;

namespace Presentation.Auth.Endpoints;

internal static partial class AuthEndpoint
{
    private static IEndpointRouteBuilder GetOauthRedirectUrlV1(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet(
                "/{provider}",
                async (
                    HttpContext ctx,
                    Config config,
                    ICommandHandler<GenerateOAuthUrlCommand, (Uri, OAuthState)> commandHandler,
                    [FromRoute] string provider,
                    [FromQuery] string? redirectPath,
                    ILoggerFactory loggerFactory
                ) =>
                {
                    var logger = loggerFactory.CreateLogger("Auth.GetOauthRedirectUrl");

                    if (!Enum.TryParse<OAuthProvider>(provider, true, out var parsedProvider))
                    {
                        Error.Validation("provider", ["Invalid oauth provider"]).ToApiError(logger);
                    }

                    NonEmptyString? additionalState = null;
                    if (redirectPath is not null)
                    {
                        var additionalStateResult = NonEmptyString.Create(redirectPath);
                        if (additionalStateResult.IsFailure)
                        {
                            return additionalStateResult.Error.ToApiError(logger);
                        }

                        additionalState = additionalStateResult.Value;
                    }

                    var command = new GenerateOAuthUrlCommand(parsedProvider, additionalState);
                    var result = await commandHandler.Handle(command);
                    if (result.IsFailure)
                    {
                        return result.Error.ToApiError(logger);
                    }

                    var cookieOptions = new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddMinutes(
                            config.Application.OAuthStateTtlMinutes
                        ),
                        HttpOnly = true,
                        Secure = config.Application.CookieSecure,
                        SameSite = SameSiteMode.Lax,
                        Path = "/",
                    };

                    ctx.Response.Cookies.Append(
                        config.Application.OAuthStateCookieName,
                        result.Value.Item2.ToString(),
                        cookieOptions
                    );

                    return Results.Redirect(result.Value.Item1.ToString());
                }
            )
            .Produces<ApiResponse<Uri>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem()
            .WithName("GetOauthRedirectUrl")
            .WithSummary("Generates OAuth redirect URL")
            .WithDescription("Returns a provider-specific OAuth redirect URL.")
            .RequireRateLimiting(RateLimitConstants.SignIn);

        return endpoint;
    }
}
