using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Schedule.DAL.Abstractions;
using Schedule.Domain.DbModels;

namespace Schedule.DAL.Implementations;

public class BaseRepository<T> : IBaseRepository<T> where T : DbEntity
{
    protected readonly ApplicationDbContext Db;

    public BaseRepository(ApplicationDbContext db)
    {
        Db = db;
    }

    public virtual IQueryable<T> GetQuery() =>
        Db.Set<T>();

    public IQueryable<T> GetQuery(Expression<Func<T, bool>> predicate) =>
        Db.Set<T>().Where(predicate).AsQueryable();

    public virtual async Task<List<T>> GetAllAsync(bool descending = false)
    {
        var data = Db.Set<T>();
        return descending ? await data.OrderByDescending(x => x).ToListAsync() : await data.ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(string id) =>
        await Db.Set<T>().FirstOrDefaultAsync(x => x.Id == id);

    public virtual async Task<List<T>> FindByAsync(Expression<Func<T, bool>> predicate) =>
        await Db.Set<T>().Where(predicate).ToListAsync();
    
    public virtual async Task<T> AddAsync(T entity)
    {
        var createdEntity = await Db.Set<T>().AddAsync(entity);
        await Db.SaveChangesAsync();
        return createdEntity.Entity;
    }
    
    public virtual async Task<T> UpdateAsync(T entity)
    {
        var entityToUpdate = await Db.Set<T>().FirstOrDefaultAsync(x => x.Id == entity.Id);
        if (entityToUpdate is null)
            throw new Exception("Entity not found");
        
        Db.Entry(entityToUpdate).CurrentValues.SetValues(entity);
        await Db.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> RemoveAsync(T entity)
    {
        try
        {
            Db.Set<T>().Remove(entity);
            await Db.SaveChangesAsync();
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
                Db.Dispose();
            }
        }

        _disposed = true;
    }

    #endregion
}