namespace Domain.Shared.Pagination;

public sealed record KeysetPaginated<T, TId>
{
    public IReadOnlyList<T> Data { get; private init; }
    public KeysetCursor<TId>? PrevCursor { get; private init; }
    public KeysetCursor<TId>? NextCursor { get; private init; }

    public KeysetPaginated(
        List<T> data,
        KeysetCursor<TId>? prevCursor,
        KeysetCursor<TId>? nextCursor
    )
    {
        Data = data.AsReadOnly();
        PrevCursor = prevCursor;
        NextCursor = nextCursor;
    }

    public KeysetPaginated(
        List<T> data,
        KeysetPagination<TId> pagination,
        Func<T, DateTimeOffset> createdAt,
        Func<T, TId> id
    )
    {
        var direction = pagination.KeysetDirection;

        var hasMore = data.Count > pagination.Limit.Value;
        if (hasMore)
        {
            data.RemoveAt(direction == KeysetDirection.Forward ? data.Count - 1 : 0);
        }

        var prevCursor = direction switch
        {
            KeysetDirection.Forward when pagination.NextCursor is not null => new KeysetCursor<TId>(
                createdAt(data[0]),
                id(data[0])
            ),

            KeysetDirection.Backward when hasMore => new KeysetCursor<TId>(
                createdAt(data[^1]),
                id(data[^1])
            ),

            _ => null,
        };

        var nextCursor = direction switch
        {
            KeysetDirection.Forward when hasMore => new KeysetCursor<TId>(
                createdAt(data[^1]),
                id(data[^1])
            ),

            KeysetDirection.Backward when pagination.PrevCursor is not null =>
                new KeysetCursor<TId>(createdAt(data[0]), id(data[0])),

            _ => null,
        };

        if (direction == KeysetDirection.Backward)
        {
            data.Reverse();
        }

        Data = data.AsReadOnly();
        PrevCursor = prevCursor;
        NextCursor = nextCursor;
    }
}
