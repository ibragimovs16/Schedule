using System.Net;
using Schedule.DAL.Abstractions;
using Schedule.Domain.DbModels;
using Schedule.Domain.Models.CreateModels;
using Schedule.Domain.Responses;

namespace Schedule.Services.Implementations;

public class SubscribersService : BaseService<DbSubscriber>
{
    public SubscribersService(IBaseRepository<DbSubscriber> repository) : base(repository)
    {
    }

    public override async Task<BaseResponse<DbSubscriber?>> AddAsync<TC>(TC model)
    {
        if (model is not SubscribersCreateModel castedModel)
            throw new ArgumentException("Invalid model type");
        
        var sub = await Repository
            .FindByAsync(s => s.TgChatId == castedModel.TgChatId && s.TgUserId == castedModel.TgUserId);

        if (sub.Count != 0)
            return new BaseResponse<DbSubscriber?>
            {
                StatusCode = HttpStatusCode.OK,
                Data = sub.First()
            };
        
        return await base.AddAsync(model);
    }
}