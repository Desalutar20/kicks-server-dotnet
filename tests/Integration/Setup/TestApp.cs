using Application.Config;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Integration.Setup;

public partial class TestApp : IAsyncLifetime, IClassFixture<ApiFactory>
{
    private readonly Config _config;
    private readonly AppDbContext _dbContext;
    private readonly HttpClient _httpClient;
    private readonly IConnectionMultiplexer _multiplexer;
    private readonly IServiceScope _scope;

    protected TestApp(ApiFactory factory)
    {
        _scope = factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _multiplexer = _scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
        _config = _scope.ServiceProvider.GetRequiredService<Config>();
        _httpClient = factory.CreateClient();
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        var tableNames = _dbContext.Model.GetEntityTypes()
                                   .Select(t => t.GetTableName())
                                   .Distinct()
                                   .ToList();

        foreach (var sql in tableNames.Select(tableName => $"TRUNCATE TABLE \"{tableName}\" CASCADE;"))
            await _dbContext.Database.ExecuteSqlRawAsync(sql);

        var db = _multiplexer.GetDatabase();

        var redisServer = _multiplexer.GetServer(_multiplexer.GetEndPoints().First());

        await foreach (var key in redisServer.KeysAsync(pattern: $"{_config.Redis.KeyPrefix}*"))
            await db.KeyDeleteAsync(key);

        await _dbContext.DisposeAsync();
        _httpClient.Dispose();
        _scope.Dispose();

        GC.SuppressFinalize(this);
    }

    private async Task<HttpResponseMessage>
        Request<TValue>(TValue data, HttpMethod method, string path, string? cookie, CancellationToken ct = default)
    {
        var message = new HttpRequestMessage(method, path);
        if (cookie is not null) message.Headers.Add("Cookie", cookie);

        message.Content = JsonContent.Create(data);

        return await _httpClient.SendAsync(message, ct);
    }

    private async Task<HttpResponseMessage>
        Request(string path, string? cookie, CancellationToken ct = default)
    {
        var message = new HttpRequestMessage(HttpMethod.Get, path);
        if (cookie is not null) message.Headers.Add("Cookie", cookie);

        return await _httpClient.SendAsync(message, ct);
    }
}