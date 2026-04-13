using Application.Abstractions.OAuth;
using Application.Auth.Errors;

namespace Application.Auth.UseCases.OAuthSignIn;

public sealed record OAuthSignInCommand(
    OAuthProvider Provider,
    NonEmptyString Code,
    OAuthState Received,
    OAuthState Expected) : ICommand<Guid>;

internal sealed class OAuthSignInCommandHandler(
    IOAuthClientFactory clientFactory,
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    IAuthCache authCache,
    Config.Config config)
    : ICommandHandler<OAuthSignInCommand, Guid>
{
    public async Task<Result<Guid>> Handle(OAuthSignInCommand command, CancellationToken ct = default)
    {
        var client = clientFactory.Get(command.Provider);
        if (!client.IsValidState(command.Received, command.Expected))
        {
            return AuthErrors.InvalidOAuthState;
        }

        var oauthUser = await client.GetUserAsync(command.Code, ct);
        if (oauthUser.IsFailure)
        {
            return Result<Guid>.Failure(oauthUser.Error);
        }

        var user = await userRepository.GetUserByEmailAsync(oauthUser.Value.Email, true, ct);
        if (user is null)
        {
            user = User.Create(oauthUser.Value.Email, null, null, null, null, null, null);
            userRepository.CreateUser(user);
        }

        var result = user.LinkOAuthProvider(command.Provider, oauthUser.Value.ProviderId);
        if (result.IsFailure)
        {
            return Result<Guid>.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(ct);

        return
            Result<Guid>.Success(await AuthService.GenerateSession(user, authCache, config.Application, ct));
    }
}