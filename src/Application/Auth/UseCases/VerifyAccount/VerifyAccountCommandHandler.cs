using Application.Auth.Errors;
using Application.Auth.Types;

namespace Application.Auth.UseCases.VerifyAccount;

public sealed record VerifyAccountCommand(NonEmptyString Token, Email Email) : ICommand;

internal sealed class VerifyAccountCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IAuthCache authCache,
    Config.Config config
) : ICommandHandler<VerifyAccountCommand>
{
    public async Task<Result> Handle(VerifyAccountCommand command, CancellationToken ct = default)
    {
        var userId = await authCache.GetUserIdByVerificationTokenAsync(command.Token, ct);
        if (userId is null)
        {
            return Result.Failure(AuthErrors.InvalidOrExpiredToken);
        }

        var user = await userRepository.GetUserByIdAsync(userId, false, ct);

        if (user is null || user.Email != command.Email)
        {
            return Result.Failure(AuthErrors.InvalidOrExpiredToken);
        }

        if (user.IsVerified)
        {
            return Result.Success();
        }

        user.ConfirmAccount();
        userRepository.UpdateUser(user);

        var sessionId = Guid.NewGuid();
        var sessionUser = user.ToSessionUser();

        await authCache.StoreSessionAsync(
            sessionUser,
            sessionId,
            TimeSpan.FromSeconds(config.Application.SessionTtlMinutes),
            ct
        );
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
