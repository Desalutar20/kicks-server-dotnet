using Application.Auth.Errors;
using Application.Auth.Types;

namespace Application.Auth.UseCases.SignIn;

public sealed record SignInCommand(Email Email, Password Password) : ICommand<Result<UserWithSessionId>>;

public class SignInCommandHandler(
    IUserRepository userRepository,
    IHashingService hashingService,
    IAuthCache authCache,
    Config.Config config)
    : ICommandHandler<SignInCommand, Result<UserWithSessionId>>
{
    private const int MaxSessionCount = 3;


    public async Task<Result<UserWithSessionId>> Handle(SignInCommand command, CancellationToken ct = default)
    {
        var user = await userRepository.GetByEmailAsync(command.Email, false, ct);

        if (user?.HashedPassword is null ||
            !hashingService.Verify(command.Password, user.HashedPassword.Value) || !user.IsValid())
            return AuthErrors.InvalidCredentials;


        var sessions = await authCache.GetAllSessionsAsync(user.Id, ct);
        if (sessions.Count >= MaxSessionCount) await authCache.DeleteSessionAsync(user.Id, sessions[0].SessionId, ct);

        var sessionId = Guid.NewGuid();
        var sessionUser = user.ToSessionUser();

        await authCache.StoreSessionAsync(sessionUser, sessionId,
            TimeSpan.FromMinutes(config.Application.SessionTtlMinutes), ct);


        return Result<UserWithSessionId>.Success(new UserWithSessionId(sessionUser, sessionId));
    }
}