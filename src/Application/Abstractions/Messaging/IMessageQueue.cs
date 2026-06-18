namespace Application.Abstractions.Messaging;

public interface IMessageQueue<T>
{
    Task<T> ReadAsync(CancellationToken ct = default);
    IAsyncEnumerable<T> ReadAllAsync(CancellationToken ct = default);
    Task WriteAsync(T data, CancellationToken ct = default);
}
