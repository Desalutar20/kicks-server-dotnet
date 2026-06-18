using System.Threading.Channels;
using Application.Abstractions.Messaging;

namespace Infrastructure.MessageQueue;

internal sealed class InMemoryMessageQueue<T> : IMessageQueue<T>
{
    private readonly Channel<T> _channel = Channel.CreateBounded<T>(100);

    public async Task<T> ReadAsync(CancellationToken ct = default) =>
        await _channel.Reader.ReadAsync(ct);

    public IAsyncEnumerable<T> ReadAllAsync(CancellationToken ct = default) =>
        _channel.Reader.ReadAllAsync(ct);

    public async Task WriteAsync(T data, CancellationToken ct = default) =>
        await _channel.Writer.WriteAsync(data, ct);
}
