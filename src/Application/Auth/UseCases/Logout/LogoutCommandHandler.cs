namespace Application.Auth.UseCases.Logout;

public sealed record LogoutCommand(UserId UserId, Guid SessionId) : ICommand;

public sealed class LogoutCommandHandler(IAuthCache authCache) : ICommandHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand command, CancellationToken ct = default)
    {
        await authCache.DeleteSessionAsync(command.UserId, command.SessionId, ct);
    }
}