using Schedule.DAL.Abstractions;
using Schedule.Domain.DbModels;
using Schedule.Domain.Exceptions;
using Schedule.Domain.Models.CreateModels;

namespace Schedule.Services.Implementations;

public class NotificationsSettingsService : BaseService<DbNotificationsSetting>
{
    private readonly IBaseRepository<DbGroup> _groupRepository;

    public NotificationsSettingsService(IBaseRepository<DbNotificationsSetting> repository, 
        IBaseRepository<DbGroup> groupRepository) : base(repository)
    {
        _groupRepository = groupRepository;
    }

    protected override DbNotificationsSetting ModelToDbModel<TT>(TT model)
    {
        if (model is not NotificationsSettingsCreateModel castedModel)
            throw new ArgumentException("Invalid model type");
        
        var group = _groupRepository
            .FindByAsync(item => item.Name == castedModel.GroupName).Result;
        if (group.Count != 1)
            throw new NotificationsSettingsExceptions.GroupNotFoundException("Группа не найдена");

        return new DbNotificationsSetting
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