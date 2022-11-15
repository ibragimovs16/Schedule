using Schedule.Backend.Controllers.BaseControllers;
using Schedule.Domain.DbModels;
using Schedule.Domain.Models.CreateModels;
using Schedule.Services.Abstractions;

namespace Schedule.Backend.Controllers;

public class NotificationsSettingsController : BaseApiController<DbNotificationsSetting, NotificationsSettingsCreateModel, NotificationsSettingsCreateModel>
{
    public NotificationsSettingsController(IBaseService<DbNotificationsSetting> service) : base(service)
    {
    }
}