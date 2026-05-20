using System.Linq.Expressions;
using System.Text.Json;
using Infrastructure.Data.Product.JsonConverters;

namespace Infrastructure.Data.Shared;

internal class RepositoryBase<T>(AppDbContext dbContext) : IRepositoryBase<T>
    where T : class, IEntity
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new BrandFilterItemConverter(), new CategoryFilterItemConverter() },
    };

    public IQueryable<T> FindAll(bool trackChanges) =>
        !trackChanges ? dbContext.Set<T>().AsNoTracking() : dbContext.Set<T>();

    public void Create(T entity)
    {
        dbContext.Set<T>().Add(entity);
    }

    public void Update(T entity)
    {
        dbContext.Set<T>().Update(entity);
    }

    public void Delete(T entity)
    {
        dbContext.Set<T>().Remove(entity);
    }

    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges) =>
        !trackChanges
            ? dbContext.Set<T>().AsNoTracking().Where(expression)
            : dbContext.Set<T>().Where(expression);

    protected static U? Deserialize<U>(object? value)
    {
        if (value is null || value is DBNull)
            return default!;

        var json = value switch
        {
            string s => s,
            JsonElement e => e.GetRawText(),
            _ => value.ToString(),
        };

        return JsonSerializer.Deserialize<U>(json!, JsonOptions)!;
    }

    protected static TU[] ParseArray<TU>(object? value)
    {
        if (value is null || value is DBNull)
            return [];

        return (TU[])value;
    }
}
