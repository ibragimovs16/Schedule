using System.Net;
using Schedule.DAL.Abstractions;
using Schedule.Domain.DbModels;
using Schedule.Domain.Responses;
using Schedule.Services.Abstractions;

namespace Schedule.Services.Implementations;

public class ParsingQueueService : IParsingQueueService
{
    private readonly IParsingQueueRepository _repository;
    private readonly IBaseRepository<DbSubscribers> _subscribersRepository;

    public ParsingQueueService(IParsingQueueRepository repository, IBaseRepository<DbSubscribers> subscribersRepository)
    {
        _repository = repository;
        _subscribersRepository = subscribersRepository;
    }

    public async Task<BaseResponse<List<DbParsingQueue>>> GetAllAsync()
    {
        try
        {
            var result = await _repository.GetAllAsync();
            return new BaseResponse<List<DbParsingQueue>>
            {
                StatusCode = HttpStatusCode.OK,
                Data = result
            };
        }
        catch (Exception e)
        {
            return new BaseResponse<List<DbParsingQueue>>
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = e.Message
            };
        }
    }

    public async Task<BaseResponse<string>> AddAsync(string group, bool isNotificationNeeded, string? subscriberId, bool isUpdating)
    {
        try
        {
            if (!string.IsNullOrEmpty(subscriberId))
            {
                var subscriber = await _subscribersRepository.GetByIdAsync(subscriberId);
                if (subscriber is null)
                    return new BaseResponse<string>
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Message = "Подписчик не найден"
                    };
            }

            var response = await _repository.AddAsync(new DbParsingQueue
            {
                GroupName = group,
                IsUpdating = isUpdating,
                IsNotificationNeeded = isNotificationNeeded,
                SubscriberId = subscriberId
            });
            return new BaseResponse<string>
            {
                StatusCode = response.IsAdded ? HttpStatusCode.OK : HttpStatusCode.Conflict,
                Data = response.Message
            };
        }
        catch (Exception e)
        {
            return new BaseResponse<string>
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = e.Message
            };
        }
    }

    public async Task<BaseResponse<bool>> RemoveAsync(DbParsingQueue entity)
    {
        try
        {
            var result = await _repository.RemoveAsync(entity);
            return new BaseResponse<bool>
            {
                StatusCode = HttpStatusCode.OK,
                Data = result
            };
        }
        catch (Exception e)
        {
            return new BaseResponse<bool>
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = e.Message
            };
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
                _repository.Dispose();
            }
        }

        _disposed = true;
    }

    #endregion
}