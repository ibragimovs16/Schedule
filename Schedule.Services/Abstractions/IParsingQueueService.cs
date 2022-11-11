using Schedule.Domain.DbModels;
using Schedule.Domain.Responses;

namespace Schedule.Services.Abstractions;

public interface IParsingQueueService : IDisposable
{
    Task<BaseResponse<List<DbParsingQueue>>> GetAllAsync();
    Task<BaseResponse<string>> AddAsync(string group, bool isNotificationNeeded, string? subscriberId, bool isUpdating);
    Task<BaseResponse<bool>> RemoveAsync(DbParsingQueue entity);
}