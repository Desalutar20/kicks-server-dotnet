using Application.Abstractions.Database;
using Application.Abstractions.Outbox;
using Domain.Users.Exceptions;

namespace Application.Auth.UseCases.SignUp;

public sealed record SignUpCommand(
    Email Email,
    Password Password,
    FirstName FirstName,
    LastName LastName,
    Gender Gender
) : ICommand;

internal sealed class SignUpCommandHandler(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    IOutboxRepository outboxRepository,
    IHashingService hashingService,
    IAuthCache authCache,
    Config.Config config
) : ICommandHandler<SignUpCommand>
{
    public async Task<Result> Handle(SignUpCommand command, CancellationToken ct = default)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

        var existingUser = await userRepository.GetUserByEmailAsync(command.Email, false, ct);
        if (existingUser is not null)
        {
            await Task.Delay(1000, ct);
            return Result.Success();
        }

        var hashedPasswordResult = hashingService.Hash(command.Password);
        if (hashedPasswordResult.IsFailure)
        {
            return hashedPasswordResult;
        }

        var tokenResult = RandomTokenGenerator.Generate();
        if (tokenResult.IsFailure)
        {
            return tokenResult;
        }

        var user = new User(
            command.Email,
            hashedPasswordResult.Value,
            command.FirstName,
            command.LastName,
            command.Gender,
            null,
            null
        );
        userRepository.CreateUser(user);

        try
        {
            await unitOfWork.SaveChangesAsync(ct);

            var message = EmailService.BuildAccountVerificationEmail(
                config.Application,
                command.Email,
                tokenResult.Value
            );
            var outbox = new Outbox(OutboxType.Email, EmailService.SerializeMessage(message));

            await authCache.StoreVerificationTokenAsync(
                user.Id,
                tokenResult.Value,
                TimeSpan.FromMinutes(config.Application.AccountVerificationTtlMinutes),
                ct
            );

            outboxRepository.CreateOutbox(outbox);
            await unitOfWork.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Result.Success();
        }
        catch (UserAlreadyExistsException)
        {
            return Result.Success();
        }
    }
}
