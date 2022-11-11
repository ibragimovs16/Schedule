using System.Linq.Expressions;
using System.Net;
using Schedule.DAL.Abstractions;
using Schedule.Domain.DbModels;
using Schedule.Domain.Models.Params;
using Schedule.Domain.Responses;
using Schedule.Services.Abstractions;

namespace Schedule.Services.Implementations;

public class BaseService<T> : IBaseService<T>
    where T : DbEntity, new()
{
    private readonly IBaseRepository<T> _repository;

    public BaseService(IBaseRepository<T> repository)
    {
        _repository = repository;
    }
    
    public virtual async Task<BaseResponse<IEnumerable<T>>> GetAllAsync()
    {
        try
        {
            var result = await _repository.GetAllAsync();
            if (result.Count == 0)
                return new BaseResponse<IEnumerable<T>>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Данные не найдены"
                };

            return new BaseResponse<IEnumerable<T>>
            {
                StatusCode = HttpStatusCode.OK,
                Data = result
            };
        }
        catch (Exception e)
        {
            return new BaseResponse<IEnumerable<T>>
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = e.Message
            };
        }
    }

    public virtual async Task<BaseResponse<T?>> FindByAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            var result = await _repository.FindByAsync(predicate);
            if (result.Count == 0)
                return new BaseResponse<T?>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Данные по заданному запросу не найдены"
                };

            return new BaseResponse<T?>
            {
                StatusCode = HttpStatusCode.OK,
                Data = result.First()
            };
        }
        catch (Exception e)
        {
            return new BaseResponse<T?>
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = e.Message
            };
        }
    }

    public virtual async Task<BaseResponse<T?>> AddAsync<TC>(TC model)
    {
        try
        {
            var dbModel = ModelToDbModel(model);
            var result = await _repository.AddAsync(dbModel);
            return new BaseResponse<T?>
            {
                StatusCode = HttpStatusCode.OK,
                Data = result
            };
        }
        catch (Exception e)
        {
            return new BaseResponse<T?>
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = e.Message
            };
        }
    }

    public virtual async Task<BaseResponse<bool>> UpdateAsync<TU>(TU model, string id)
    {
        try
        {
            var result = await _repository.FindByAsync(item => item.Id == id);
        
            if (result.Count == 0)
                return new BaseResponse<bool>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Указанный элемент не найден"
                };
        
            var dbModel = ModelToDbModel(model);
            dbModel.Id = id;

            var updatedModel = await _repository.UpdateAsync(dbModel);
            if (!updatedModel.Equals(dbModel))
                return new BaseResponse<bool>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Не удалось обновить элемент"
                };

            return new BaseResponse<bool>
            {
                StatusCode = HttpStatusCode.OK,
                Data = true
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

    public virtual async Task<BaseResponse<bool>> RemoveAsync(string id)
    {
        try
        {
            var result = await _repository.FindByAsync(bs => bs.Id == id);
            if (result.Count == 0)
                return new BaseResponse<bool>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Указанный элемент не найден"
                };
        
            var dbModel = result.First();
            var removed = await _repository.RemoveAsync(dbModel);
            if (!removed)
                return new BaseResponse<bool>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Не удалось удалить элемент"
                };

            return new BaseResponse<bool>
            {
                StatusCode = HttpStatusCode.OK,
                Data = true
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

    protected virtual T ModelToDbModel<TT>(TT model)
    {
        var dbModel = new T();

        var dbModelProps = typeof(T).GetProperties();
        var modelProps = typeof(TT).GetProperties();
        
        foreach (var prop in modelProps)
        {
            var dbProp = dbModelProps.FirstOrDefault(p => p.Name == prop.Name);
            if (dbProp != null)
                dbProp.SetValue(dbModel, prop.GetValue(model));
        }

        return dbModel;
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