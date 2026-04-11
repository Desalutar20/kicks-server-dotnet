using Application.Auth.Errors;
using Application.Auth.Types;

namespace Application.Auth.UseCases.VerifyAccount;

public sealed record VerifyAccountCommand(NonEmptyString Token, Email Email) : ICommand<Result<UserWithSessionId>>;

internal sealed class VerifyAccountCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IAuthCache authCache,
    Config.Config config)
    : ICommandHandler<VerifyAccountCommand, Result<UserWithSessionId>>
{
    public async Task<Result<UserWithSessionId>> Handle(VerifyAccountCommand command, CancellationToken ct = default)
    {
        var userId = await authCache.GetUserIdByVerificationTokenAsync(command.Token, ct);
        if (userId is null) return Result<UserWithSessionId>.Failure(AuthErrors.InvalidOrExpiredToken);

        var user = await userRepository.GetByIdAsync(userId.Value, false, ct);

        if (user is null || user.Email != command.Email)
            return Result<UserWithSessionId>.Failure(AuthErrors.InvalidOrExpiredToken);
        if (user.IsVerified)
            return Result<UserWithSessionId>.Success(new UserWithSessionId(user.ToSessionUser(), null));

        user.ConfirmAccount();
        userRepository.UpdateUser(user);

        var sessionId = Guid.NewGuid();
        var sessionUser = user.ToSessionUser();

        await authCache.StoreSessionAsync(sessionUser, sessionId,
            TimeSpan.FromSeconds(config.Application.SessionTtlMinutes), ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<UserWithSessionId>.Success(new UserWithSessionId(sessionUser, sessionId));
    }
}