using Domain.Outbox;

namespace Application.Auth.UseCases.SignUp;

public sealed record SignUpCommand(
    Email Email,
    Password Password,
    FirstName FirstName,
    LastName LastName,
    Gender Gender) : ICommand;

internal sealed class SignUpCommandHandler(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    IOutboxRepository outboxRepository,
    IHashingService hashingService,
    IAuthCache authCache,
    Config.Config config)
    : ICommandHandler<SignUpCommand>
{
    public async Task<Result> Handle(SignUpCommand command, CancellationToken ct = default)
    {
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


        var user = User.Create(command.Email, hashedPasswordResult.Value, command.FirstName, command.LastName,
            command.Gender, null, null);


        var message = EmailService.BuildAccountVerificationEmail(config.Application, command.Email, tokenResult.Value);
        var data = EmailService.SerializeMessage(message);
        var outbox = Outbox.Create(OutboxType.Email, NonEmptyString.Create(data)
                                                                   .Value);

        await authCache.StoreVerificationTokenAsync(user.Id, tokenResult.Value,
            TimeSpan.FromMinutes(config.Application.AccountVerificationTtlMinutes), ct);

        userRepository.CreateUser(user);
        outboxRepository.CreateOutbox(outbox);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}