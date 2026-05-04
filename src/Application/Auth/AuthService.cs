using Application.Auth.Types;
using Application.Config;

namespace Application.Auth;

internal static class AuthService
{
    private const int MaxSessionCount = 3;

    public static async Task<Guid> GenerateSession(
        User user,
        IAuthCache authCache,
        ApplicationConfig applicationConfig,
        CancellationToken ct = default
    )
    {
        var sessions = await authCache.GetAllSessionsAsync(user.Id, ct);
        if (sessions.Count >= MaxSessionCount)
        {
            await authCache.DeleteSessionAsync(user.Id, sessions[0].SessionId, ct);
        }

        var sessionId = Guid.NewGuid();
        var sessionUser = user.ToSessionUser();

        await authCache.StoreSessionAsync(
            sessionUser,
            sessionId,
            TimeSpan.FromMinutes(applicationConfig.SessionTtlMinutes),
            ct
        );

        return sessionId;
    }
}
