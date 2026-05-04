namespace Domain.Shared.Pagination;

public sealed record KeysetPaginated<T, TId>(
    List<T> Data,
    KeysetCursor<TId>? PrevCursor,
    KeysetCursor<TId>? NextCursor
);
