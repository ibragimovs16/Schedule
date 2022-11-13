using Microsoft.EntityFrameworkCore;
using Schedule.DAL.Abstractions;
using Schedule.Domain.DbModels;

namespace Schedule.DAL.Implementations;

public class ParsingQueueRepository : IParsingQueueRepository
{
    private readonly ApplicationDbContext _db;

    public ParsingQueueRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<DbParsingQueue>> GetAllAsync() =>
        await _db.ParsingQueue.ToListAsync();

    public async Task<(string Message, bool IsAdded)> AddAsync(DbParsingQueue entity)
    {
        if (!entity.IsUpdating)
        {
            var group = await _db.Groups.FirstOrDefaultAsync(item => item.Name == entity.GroupName);
            if (group is not null)
                return ("Расписание для этой группы уже существует, обновление расписания происходит каждый день в 00:00 (по МСК)", false);
            
            await _db.ParsingQueue.AddAsync(entity);
            await _db.SaveChangesAsync();
            return ("Группа добавлена в очередь", true);
        }
        
        await _db.ParsingQueue.AddAsync(entity);
        await _db.SaveChangesAsync();
        return (string.Empty, false);
    }

    public async Task<bool> RemoveAsync(DbParsingQueue entity)
    {
        try
        {
            _db.ParsingQueue.Remove(entity);
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