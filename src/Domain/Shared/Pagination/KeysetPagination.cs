namespace Domain.Shared.Pagination;

public sealed record KeysetPagination<T>(
    PositiveInt Limit,
    KeysetCursor<T>? PrevCursor,
    KeysetCursor<T>? NextCursor
)
{
    public KeysetDirection KeysetDirection =>
        PrevCursor is not null ? KeysetDirection.Backward : KeysetDirection.Forward;

    public KeysetCursor<T>? Cursor => PrevCursor ?? NextCursor;
}
