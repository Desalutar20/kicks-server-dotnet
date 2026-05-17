using Application.Abstractions.Cache;
using Domain.Shared;

namespace Infrastructure.Cache;

internal sealed class Redis(IConnectionMultiplexer cache, Config config) : ICachingService
{
    public async Task SetAsync(
        NonEmptyString key,
        NonEmptyString data,
        TimeSpan? exp,
        CancellationToken ct = default
    )
    {
        var db = cache.GetDatabase();
        var k = GenerateKey(key.Value);

        await (
            exp is not null
                ? db.StringSetAsync(k, data.Value, exp.Value).WaitAsync(ct)
                : db.StringSetAsync(k, data.Value)
        ).WaitAsync(ct);
    }

    public async Task<string?> GetAsync(
        NonEmptyString key,
        TimeSpan? exp,
        CancellationToken ct = default
    )
    {
        var db = cache.GetDatabase();
        var k = GenerateKey(key.Value);

        var data = await (
            exp is not null
                ? db.StringGetSetExpiryAsync(k, exp).WaitAsync(ct)
                : db.StringGetAsync(k)
        ).WaitAsync(ct);

        return data.HasValue ? data.ToString() : null;
    }

    public async Task DeleteAsync(NonEmptyString key, CancellationToken ct = default)
    {
        var db = cache.GetDatabase();
        var k = GenerateKey(key.Value);

        await db.KeyDeleteAsync(k).WaitAsync(ct);
    }

    public async Task<string?> GetDelAsync(NonEmptyString key, CancellationToken ct = default)
    {
        var db = cache.GetDatabase();
        var k = GenerateKey(key.Value);

        var data = await db.StringGetDeleteAsync(k).WaitAsync(ct);

        return data.HasValue ? data.ToString() : null;
    }

    public async Task<bool> ExistsAsync(NonEmptyString key, CancellationToken ct = default)
    {
        var db = cache.GetDatabase();
        var k = GenerateKey(key.Value);

        return await db.KeyExistsAsync(k).WaitAsync(ct);
    }

    public async Task RefreshAsync(NonEmptyString key, TimeSpan exp, CancellationToken ct = default)
    {
        var db = cache.GetDatabase();
        var k = GenerateKey(key.Value);

        await db.KeyExpireAsync(k, exp).WaitAsync(ct);
    }

    private string GenerateKey(string key) => $"{config.Redis.KeyPrefix ?? ""}{key}";
}
