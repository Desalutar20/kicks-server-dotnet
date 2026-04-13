namespace Application.Auth.UseCases.Logout;

public sealed record LogoutCommand(UserId UserId, Guid SessionId) : ICommand;

internal sealed class LogoutCommandHandler(IAuthCache authCache) : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand command, CancellationToken ct = default)
    {
        await authCache.DeleteSessionAsync(command.UserId, command.SessionId, ct);

        return Result.Success();
    }
}