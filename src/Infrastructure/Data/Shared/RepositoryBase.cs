using System.Linq.Expressions;

namespace Infrastructure.Data.Shared;

internal class RepositoryBase<T>(AppDbContext dbContext) : IRepositoryBase<T>
    where T : class, IEntity
{
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
}
