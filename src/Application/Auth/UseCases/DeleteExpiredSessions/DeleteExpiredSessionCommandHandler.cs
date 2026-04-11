namespace Application.Auth.UseCases.DeleteExpiredSessions;

public sealed record DeleteExpiredSessionCommand : ICommand;

internal sealed class DeleteExpiredSessionCommandHandler(IAuthCache cache)
    : ICommandHandler<DeleteExpiredSessionCommand>
{
    public async Task Handle(DeleteExpiredSessionCommand command, CancellationToken ct = default)
    {
        await cache.DeleteExpiredSessionsAsync(ct);
    }
}