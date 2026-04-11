using Application.Auth.Errors;
using Application.Auth.Types;

namespace Application.Auth.UseCases.Authenticate;

public record AuthenticateCommand(Guid SessionId) : ICommand<Result<SessionUser>>;

internal sealed class AuthenticateCommandHandler(IAuthCache cache)
    : ICommandHandler<AuthenticateCommand, Result<SessionUser>>
{
    public async Task<Result<SessionUser>> Handle(AuthenticateCommand command, CancellationToken ct = default)
    {
        var sessionUser = await cache.GetSessionAsync(command.SessionId, ct);
        return sessionUser is null ? AuthErrors.Unauthorized : Result<SessionUser>.Success(sessionUser);
    }
}