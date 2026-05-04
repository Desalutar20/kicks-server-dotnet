using Application.Auth.Errors;
using Application.Auth.Types;

namespace Application.Auth.UseCases.SignIn;

public sealed record SignInCommand(Email Email, Password Password) : ICommand<UserWithSessionId>;

public class SignInCommandHandler(
    IUserRepository userRepository,
    IHashingService hashingService,
    IAuthCache authCache,
    Config.Config config
) : ICommandHandler<SignInCommand, UserWithSessionId>
{
    public async Task<Result<UserWithSessionId>> Handle(
        SignInCommand command,
        CancellationToken ct = default
    )
    {
        var user = await userRepository.GetUserByEmailAsync(command.Email, false, ct);

        if (
            user?.HashedPassword is null
            || !hashingService.Verify(command.Password, user.HashedPassword)
            || !user.IsValid()
        )
        {
            return AuthErrors.InvalidCredentials;
        }

        var sessionId = await AuthService.GenerateSession(user, authCache, config.Application, ct);

        return Result<UserWithSessionId>.Success(
            new UserWithSessionId(user.ToSessionUser(), sessionId)
        );
    }
}
