using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Schedule.DAL.Abstractions;
using Schedule.Domain.DbModels;

namespace Schedule.DAL.Implementations;

public class BaseRepository<T> : IBaseRepository<T> where T : DbEntity
{
    private readonly ApplicationDbContext _db;

    public BaseRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public virtual IQueryable<T> GetQuery() =>
        _db.Set<T>();

    public IQueryable<T> GetQuery(Expression<Func<T, bool>> predicate) =>
        _db.Set<T>().Where(predicate).AsQueryable();

    public virtual async Task<List<T>> GetAllAsync(bool descending = false)
    {
        var data = _db.Set<T>();
        return descending ? await data.OrderByDescending(x => x).ToListAsync() : await data.ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(string id) =>
        await _db.Set<T>().FirstOrDefaultAsync(x => x.Id == id);

    public virtual async Task<List<T>> FindByAsync(Expression<Func<T, bool>> predicate) =>
        await _db.Set<T>().Where(predicate).ToListAsync();
    
    public virtual async Task<T> AddAsync(T entity)
    {
        var createdEntity = await _db.Set<T>().AddAsync(entity);
        await _db.SaveChangesAsync();
        return createdEntity.Entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        var updatedEntity = _db.Set<T>().Update(entity);
        await _db.SaveChangesAsync();
        return updatedEntity.Entity;
    }
    public virtual async Task<bool> RemoveAsync(T entity)
    {
        try
        {
            _db.Set<T>().Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    #region Dispose

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _db.Dispose();
            }
        }

        _disposed = true;
    }

    #endregion
}