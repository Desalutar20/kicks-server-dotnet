using System.Linq.Expressions;
using Domain.Shared.Pagination;

namespace Infrastructure.Data.Extensions;

public static class QueryExtensions
{
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, bool>> predicate
    ) => condition ? query.Where(predicate) : query;

    public static IQueryable<T> ApplyKeysetPagination<T, TId>(
        this IQueryable<T> query,
        KeysetPagination<TId> pagination
    )
        where T : IEntity<TId>
    {
        var direction = pagination.KeysetDirection;
        var cursor = pagination.Cursor;

        if (cursor is not null)
        {
            query = query
                .WhereIf(
                    direction == KeysetDirection.Backward,
                    x =>
                        EF.Functions.GreaterThan(
                            ValueTuple.Create(x.CreatedAt, x.Id),
                            ValueTuple.Create(cursor.CreatedAt, cursor.Id)
                        )
                )
                .WhereIf(
                    direction == KeysetDirection.Forward,
                    x =>
                        EF.Functions.LessThan(
                            ValueTuple.Create(x.CreatedAt, x.Id),
                            ValueTuple.Create(cursor.CreatedAt, cursor.Id)
                        )
                );
        }

        query = direction switch
        {
            KeysetDirection.Backward => query.OrderBy(u => u.CreatedAt).ThenBy(u => u.Id),
            _ => query.OrderByDescending(u => u.CreatedAt).ThenByDescending(u => u.Id),
        };

        return query.Take(pagination.Limit.Value + 1);
    }

    public static IQueryable<T> WhereNotNull<T, TProp>(
        this IQueryable<T> query,
        TProp? value,
        Expression<Func<T, bool>> predicate
    ) => value is null ? query : query.Where(predicate);
}
