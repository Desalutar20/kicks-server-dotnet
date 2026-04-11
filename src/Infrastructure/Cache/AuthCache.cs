using System.Text.Json;
using Application.Abstractions.Cache;
using Application.Auth.JsonConverters;
using Application.Auth.Types;
using Domain.Shared;
using NRedisStack;

namespace Infrastructure.Cache;

internal sealed class AuthCache(IConnectionMultiplexer multiplexer, Config config) : IAuthCache
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new SessionUserConverter()
        }
    };

    private readonly string _prefix = config.Redis.KeyPrefix ?? "";

    public async Task StoreVerificationTokenAsync(UserId userId, NonEmptyString token, TimeSpan ttl,
        CancellationToken ct = default) =>
        await StoreToken(TokenType.AccountVerification, userId, token, ttl, ct);

    public async Task<UserId?>
        GetUserIdByVerificationTokenAsync(NonEmptyString token, CancellationToken ct = default) =>
        await GetUserIdByToken(TokenType.AccountVerification, token, ct);

    public async Task StorePasswordResetTokenAsync(UserId userId, NonEmptyString token, TimeSpan ttl,
        CancellationToken ct = default) =>
        await StoreToken(TokenType.ResetPassword, userId, token, ttl, ct);

    public async Task<UserId?>
        GetUserIdByPasswordResetTokenAsync(NonEmptyString token, CancellationToken ct = default) =>
        await GetUserIdByToken(TokenType.ResetPassword, token, ct);


    public async Task StoreSessionAsync(SessionUser sessionUser, Guid sessionId, TimeSpan ttl,
        CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(sessionUser, JsonOptions);

        var db = multiplexer.GetDatabase();
        var pipeline = new Pipeline(db);

        var expireAt = DateTimeOffset.UtcNow.Add(ttl).ToUnixTimeSeconds();
        var sessionIdString = sessionId.ToString();

        var sessionKey = GenerateSessionKey(sessionIdString);
        var userSessionsKey = GenerateSessionsKey(sessionUser.Id);

        var t1 = pipeline.Db.StringSetAsync(sessionKey, json, ttl);
        var t2 = pipeline.Db.SortedSetAddAsync(userSessionsKey, sessionIdString, expireAt);

        pipeline.Execute();

        await Task.WhenAll(t1, t2).WaitAsync(ct);
    }


    public async Task<IReadOnlyList<(Guid SessionId, DateTime CreatedAt)>> GetAllSessionsAsync(UserId userId,
        CancellationToken ct = default)
    {
        var db = multiplexer.GetDatabase();
        var userSessionsKey = GenerateSessionsKey(userId);

        var entries = await db.SortedSetRangeByScoreWithScoresAsync(userSessionsKey, order: Order.Ascending);

        return entries
               .Select(e => (
                   SessionId: Guid.Parse(e.Element.ToString()),
                   CreatedAt: DateTimeOffset.FromUnixTimeSeconds((long)e.Score).UtcDateTime
               ))
               .ToList();
    }


    public async Task<SessionUser?> GetSessionAsync(Guid sessionId, CancellationToken ct = default)
    {
        var db = multiplexer.GetDatabase();

        var value = await db.StringGetAsync(GenerateSessionKey(sessionId.ToString())).WaitAsync(ct);

        return value.HasValue
            ? JsonSerializer.Deserialize<SessionUser>(value.ToString(), JsonOptions)
            : null;
    }

    public async Task DeleteSessionAsync(UserId userId, Guid sessionId, CancellationToken ct = default)
    {
        var db = multiplexer.GetDatabase();
        var pipeline = new Pipeline(db);

        var sessionKey = GenerateSessionKey(sessionId.ToString());
        var userSessionsKey = GenerateSessionsKey(userId);

        var t1 = pipeline.Db.KeyDeleteAsync(sessionKey);
        var t2 = pipeline.Db.SortedSetRemoveAsync(userSessionsKey, sessionId.ToString());

        pipeline.Execute();
        await Task.WhenAll(t1, t2).WaitAsync(ct);
    }

    public async Task DeleteAllSessionsAsync(UserId userId, CancellationToken ct = default)
    {
        var db = multiplexer.GetDatabase();
        var userSessionsKey = GenerateSessionsKey(userId);

        var sessionIds = await db.SortedSetRangeByScoreAsync(userSessionsKey);

        var tasks = sessionIds
                    .Select(id => db.KeyDeleteAsync(GenerateSessionKey(id!)))
                    .ToList();

        tasks.Add(db.KeyDeleteAsync(userSessionsKey));

        await Task.WhenAll(tasks).WaitAsync(ct);
    }

    public async Task DeleteExpiredSessionsAsync(CancellationToken ct = default)
    {
        var db = multiplexer.GetDatabase();

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var server = multiplexer.GetServer(multiplexer.GetEndPoints().First());

        await foreach (var userSessionsKey in server.KeysAsync(pattern: $"{_prefix}{CacheConstants.SessionsPrefix}*")
                                                    .WithCancellation(ct))
        {
            var expiredSessions = await db.SortedSetRangeByScoreAsync(
                userSessionsKey,
                double.NegativeInfinity,
                now);

            if (expiredSessions.Length == 0) continue;


            var tasks = new List<Task>();

            foreach (var sessionId in expiredSessions)
            {
                var id = sessionId.ToString();

                tasks.Add(db.KeyDeleteAsync(GenerateSessionKey(id)));
                tasks.Add(db.SortedSetRemoveAsync(userSessionsKey, id));
            }

            await Task.WhenAll(tasks).WaitAsync(ct);
        }
    }

    private async Task<UserId?> GetUserIdByToken(TokenType tokenType, NonEmptyString token, CancellationToken ct)
    {
        var db = multiplexer.GetDatabase();

        var cacheKey = tokenType switch
        {
            TokenType.AccountVerification => GenerateAccountVerificationKey(token),
            TokenType.ResetPassword => GenerateResetPasswordKey(token),
            _ => throw new ArgumentException()
        };

        var userId = await db.StringGetDeleteAsync(cacheKey).WaitAsync(ct);

        if (!userId.HasValue || !Guid.TryParse(userId.ToString(), out var parsed)) return null;

        return new UserId(parsed);
    }

    private async Task StoreToken(TokenType tokenType, UserId userId, NonEmptyString token, TimeSpan ttl,
        CancellationToken ct)
    {
        var db = multiplexer.GetDatabase();

        var cacheKey = tokenType switch
        {
            TokenType.AccountVerification => GenerateAccountVerificationKey(token),
            TokenType.ResetPassword => GenerateResetPasswordKey(token),
            _ => throw new ArgumentException()
        };

        await db.StringSetAsync(cacheKey, userId.Value.ToString(), ttl);
    }

    private string GenerateAccountVerificationKey(NonEmptyString token) =>
        $"{_prefix}{CacheConstants.AccountVerificationPrefix}{token}";

    private string GenerateResetPasswordKey(NonEmptyString token) =>
        $"{_prefix}{CacheConstants.ResetPasswordPrefix}{token.Value}";

    private string GenerateSessionKey(string sessionId) => $"{_prefix}{CacheConstants.SessionPrefix}{sessionId}";

    private string GenerateSessionsKey(UserId userId) => $"{_prefix}{CacheConstants.SessionsPrefix}{userId.Value}";

    private enum TokenType
    {
        AccountVerification,
        ResetPassword
    }
}