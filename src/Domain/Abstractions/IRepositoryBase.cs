using System.Linq.Expressions;

namespace Domain.Abstractions;

public interface IRepositoryBase<T> where T : class, IEntity
{
    IQueryable<T> FindAll(bool trackChanges);

    IQueryable<T> FindByCondition(Expression<Func<
        T, bool>> expression, bool trackChanges);

    void Create(T entity);
    void Update(T entity);
    void Delete(T entity);
}