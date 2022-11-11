using System.Linq.Expressions;
using Schedule.Domain.DbModels;

namespace Schedule.DAL.Abstractions;

public interface IBaseRepository<T> : IDisposable where T : DbEntity
{
    IQueryable<T> GetQuery();
    IQueryable<T> GetQuery(Expression<Func<T, bool>> predicate);
    Task<List<T>> GetAllAsync(bool descending = false);
    Task<T?> GetByIdAsync(string id);
    Task<List<T>> FindByAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> RemoveAsync(T entity);
}