using Infrastructure.Cache;

namespace Integration.Setup;

public partial class TestApp
{
    protected async Task<string?> GetRedisToken(TokenType type, CancellationToken ct = default)
    {
        var server = _multiplexer.GetServer(_multiplexer.GetEndPoints().First());
        var pattern = GeneratePattern(type);

        await foreach (var key in server.KeysAsync(pattern: $"{pattern}*").WithCancellation(ct))
            return key.ToString().Split(pattern)[1];

        return null;
    }

    protected async Task DeleteRedisAccountVerificationToken(
        TokenType type,
        CancellationToken ct = default
    )
    {
        var db = _multiplexer.GetDatabase();
        var token = await GetRedisToken(type, ct);

        var pattern = GeneratePattern(type);

        await db.KeyDeleteAsync($"{pattern}{token}");
    }

    private string GeneratePattern(TokenType type)
    {
        var prefix = type switch
        {
            TokenType.AccountVerification => CacheConstants.AccountVerificationPrefix,
            _ => CacheConstants.ResetPasswordPrefix,
        };

        var pattern = $"{_config.Redis.KeyPrefix}{prefix}";

        return pattern;
    }

    protected enum TokenType
    {
        AccountVerification,
        ResetPassword,
    }
}
