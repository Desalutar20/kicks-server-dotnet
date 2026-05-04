using Application.Auth.Errors;
using Domain.Outbox;

namespace Application.Auth.UseCases.ResetPassword;

public sealed record ResetPasswordCommand(NonEmptyString Token, Email Email, Password NewPassword)
    : ICommand;

internal sealed class ResetPasswordHandler(
    IUserRepository userRepository,
    IOutboxRepository outboxRepository,
    IUnitOfWork unitOfWork,
    IAuthCache authCache,
    IHashingService hashingService
) : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken ct = default)
    {
        var userId = await authCache.GetUserIdByPasswordResetTokenAsync(command.Token, ct);
        if (userId is null)
        {
            return Result.Failure(AuthErrors.InvalidOrExpiredToken);
        }

        var user = await userRepository.GetUserByIdAsync(userId, false, ct);
        if (user is null || user.Email != command.Email || !user.IsValid())
        {
            return Result.Failure(AuthErrors.InvalidOrExpiredToken);
        }

        var hashedPasswordResult = hashingService.Hash(command.NewPassword);
        if (hashedPasswordResult.IsFailure)
        {
            return hashedPasswordResult;
        }

        var message = EmailService.BuildPasswordChangedEmail(user.Email);
        var data = EmailService.SerializeMessage(message);
        var outbox = Outbox.Create(OutboxType.Email, NonEmptyString.Create(data).Value);

        user.UpdatePassword(hashedPasswordResult.Value);

        userRepository.UpdateUser(user);
        outboxRepository.CreateOutbox(outbox);

        await authCache.DeleteAllSessionsAsync(user.Id, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
