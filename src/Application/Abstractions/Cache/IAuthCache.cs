using Application.Auth.Types;

namespace Application.Abstractions.Cache;

public interface IAuthCache
{
    Task StoreVerificationTokenAsync(UserId userId, NonEmptyString token, TimeSpan ttl, CancellationToken ct = default);
    Task<UserId?> GetUserIdByVerificationTokenAsync(NonEmptyString token, CancellationToken ct = default);

    Task StorePasswordResetTokenAsync(UserId userId, NonEmptyString token, TimeSpan ttl,
        CancellationToken ct = default);

    Task<UserId?> GetUserIdByPasswordResetTokenAsync(NonEmptyString token, CancellationToken ct = default);


    Task StoreSessionAsync(SessionUser sessionUser, Guid sessionId, TimeSpan ttl, CancellationToken ct = default);
    Task<SessionUser?> GetSessionAsync(Guid sessionId, CancellationToken ct = default);

    Task<IReadOnlyList<(Guid SessionId, DateTime CreatedAt)>> GetAllSessionsAsync(UserId userId,
        CancellationToken ct = default);

    Task DeleteSessionAsync(UserId userId, Guid sessionId, CancellationToken ct = default);
    Task DeleteAllSessionsAsync(UserId userId, CancellationToken ct = default);
    Task DeleteExpiredSessionsAsync(CancellationToken ct = default);
}