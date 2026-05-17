using Domain.Outbox;

namespace Application.Auth.UseCases.ForgotPassword;

public sealed record ForgotPasswordCommand(Email Email) : ICommand;

internal sealed class ForgotPasswordCommandHandler(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    IOutboxRepository outboxRepository,
    IAuthCache authCache,
    Config.Config config
) : ICommandHandler<ForgotPasswordCommand>
{
    public async Task<Result> Handle(ForgotPasswordCommand command, CancellationToken ct = default)
    {
        var user = await userRepository.GetUserByEmailAsync(command.Email, false, ct);
        if (user is null || !user.IsValid())
        {
            await Task.Delay(1000, ct);
            return Result.Success();
        }

        var tokenResult = RandomTokenGenerator.Generate();
        if (tokenResult.IsFailure)
        {
            return Result.Failure(Error.Internal("Something went wrong"));
        }

        var message = EmailService.BuildResetPasswordEmail(
            config.Application,
            command.Email,
            tokenResult.Value
        );
        var outbox = Outbox.Create(OutboxType.Email, EmailService.SerializeMessage(message));

        await authCache.StorePasswordResetTokenAsync(
            user.Id,
            tokenResult.Value,
            TimeSpan.FromMinutes(config.Application.ResetPasswordTtlMinutes),
            ct
        );

        outboxRepository.CreateOutbox(outbox);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
