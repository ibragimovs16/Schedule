using Schedule.Domain.DbModels;

namespace Schedule.DAL.Abstractions;

public interface IParsingQueueRepository : IDisposable
{
    Task<List<DbParsingQueue>> GetAllAsync();
    Task<string> AddAsync(DbParsingQueue entity);
    Task<bool> RemoveAsync(DbParsingQueue entity);
}