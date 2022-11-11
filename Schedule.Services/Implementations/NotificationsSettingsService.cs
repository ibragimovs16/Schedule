using Schedule.DAL.Abstractions;
using Schedule.Domain.DbModels;
using Schedule.Domain.Exceptions;
using Schedule.Domain.Models.CreateModels;

namespace Schedule.Services.Implementations;

public class NotificationsSettingsService : BaseService<DbNotificationsSettings>
{
    private readonly IBaseRepository<DbGroup> _groupRepository;

    public NotificationsSettingsService(IBaseRepository<DbNotificationsSettings> repository, 
        IBaseRepository<DbGroup> groupRepository) : base(repository)
    {
        _groupRepository = groupRepository;
    }

    protected override DbNotificationsSettings ModelToDbModel<TT>(TT model)
    {
        if (model is not NotificationsSettingsCreateModel castedModel)
            throw new Exception("Invalid model type");
        
        var group = _groupRepository
            .FindByAsync(item => item.Name == castedModel.GroupName).Result;
        if (group.Count != 1)
            throw new NotificationsSettingsExceptions.GroupNotFoundException("Группа не найдена");

        return new DbNotificationsSettings
        {
            GroupId = group.First().Id,
            DayOfWeek = castedModel.DayOfWeek,
            Time = castedModel.Time,
            IsScheduleNotifyNeeded = castedModel.IsScheduleNotifyNeeded,
            IsEnabled = castedModel.IsEnabled,
            SubscriberId = castedModel.SubscriberId
        };
    }
}