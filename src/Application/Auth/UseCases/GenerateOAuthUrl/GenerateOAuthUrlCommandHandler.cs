using Application.Abstractions.OAuth;

namespace Application.Auth.UseCases.GenerateOAuthUrl;

public sealed record GenerateOAuthUrlCommand(
    OAuthProvider Provider,
    NonEmptyString? AdditionalState
) : ICommand<(Uri, OAuthState)>;

internal sealed class GenerateOAuthUrlCommandHandler(IOAuthClientFactory clientFactory)
    : ICommandHandler<GenerateOAuthUrlCommand, (Uri, OAuthState)>
{
    public async Task<Result<(Uri, OAuthState)>> Handle(
        GenerateOAuthUrlCommand command,
        CancellationToken ct = default
    )
    {
        var client = clientFactory.Get(command.Provider);
        var state = new OAuthState(Guid.NewGuid(), command.AdditionalState);

        return await Task.FromResult(
            Result<(Uri, OAuthState)>.Success((client.GenerateRedirectUrl(state), state))
        );
    }
}
