using Domain.Shared.Pagination;

namespace Infrastructure.Data.Extensions;

public static class KeysetExtensions
{
    public static KeysetPaginated<T, TId> ToKeysetPaginated<T, TId>(
        this List<T> data,
        KeysetPagination<TId> pagination,
        Func<T, DateTimeOffset> createdAt,
        Func<T, TId> id
    )
    {
        var direction = pagination.KeysetDirection;

        var hasMore = data.Count > pagination.Limit.Value;
        if (hasMore)
        {
            data.RemoveAt(data.Count - 1);
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

        return new KeysetPaginated<T, TId>(data, prevCursor, nextCursor);
    }
}
