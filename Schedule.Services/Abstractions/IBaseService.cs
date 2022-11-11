using System.Linq.Expressions;
using Schedule.Domain.DbModels;
using Schedule.Domain.Models.Params;
using Schedule.Domain.Responses;

namespace Schedule.Services.Abstractions;

public interface IBaseService<T> : IDisposable
    where T : DbEntity, new()
{
    Task<BaseResponse<IEnumerable<T>>> GetAllAsync();
    Task<BaseResponse<T?>> FindByAsync(Expression<Func<T, bool>> predicate);
    Task<BaseResponse<T?>> AddAsync<TC>(TC model);
    Task<BaseResponse<bool>> UpdateAsync<TU>(TU model, string id);
    Task<BaseResponse<bool>> RemoveAsync(string id);
}