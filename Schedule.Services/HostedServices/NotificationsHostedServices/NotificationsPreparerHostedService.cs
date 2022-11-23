using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Schedule.Domain.DbModels;
using Schedule.Domain.DbModels.DbEnums;
using Schedule.Domain.Models.CreateModels;
using Schedule.Domain.Models.Params;
using Schedule.Services.Abstractions;
using Schedule.Services.HostedServices.BaseHostedServices;

namespace Schedule.Services.HostedServices.NotificationsHostedServices;

public class NotificationsPreparerHostedService : ScopedProcessor
{
    private readonly string _adminId;

    public NotificationsPreparerHostedService(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        : base(serviceScopeFactory, TimeSpan.FromSeconds(20))
    {
        _adminId = configuration["AdminId"];
    }

    protected override async Task ProcessInScope(IServiceProvider scope)
    {
        using var notificationsSettingsService = scope.GetRequiredService<IBaseService<DbNotificationsSetting>>();
        using var notificationsService = scope.GetRequiredService<IBaseService<DbNotification>>();
        using var scheduleService = scope.GetRequiredService<IScheduleService>();
        using var groupService = scope.GetRequiredService<IBaseService<DbGroup>>();

        try
        {
            var currentTime = DateTime.Now.ToString("HH:mm");
            var notificationsSettings = await notificationsSettingsService.GetAllAsync();
            if (notificationsSettings.StatusCode != HttpStatusCode.OK)
                throw new Exception("Notifications settings not found");

            var currentNotificationsSettings = notificationsSettings.Data!
                .Where(ns => ns.IsScheduleNotifyNeeded &&
                             ns.DayOfWeek == (int)DateTime.Now.DayOfWeek &&
                             ns.Time == currentTime)
                .ToList();

            if (!currentNotificationsSettings.Any())
                return;

            foreach (var notificationSetting in currentNotificationsSettings)
            {
                var groupNameResult = await groupService
                    .FindByAsync(g => g.Id == notificationSetting.GroupId);
                if (groupNameResult.StatusCode != HttpStatusCode.OK)
                    throw new Exception(groupNameResult.Message);
                var date = DateTime.Now
                    .AddDays((int) DayOfWeek.Saturday - (int) DateTime.Now.DayOfWeek)
                    .ToString("dd.MM.yyyy");
                
                var schedule = await scheduleService.GetAllAsync(new ScheduleParams
                {
                    GroupName = groupNameResult.Data!.Name,
                    Date = date
                });
                
                if (schedule.StatusCode != HttpStatusCode.OK)
                    throw new Exception(schedule.Message);

                var createResult = await notificationsService.AddAsync(
                    new NotificationCreateModel
                    {
                        Message = JsonConvert.SerializeObject(schedule.Data),
                        SubscriberId = notificationSetting.SubscriberId,
                        Type = NotificationsTypes.Schedule
                    });
                if (createResult.StatusCode != HttpStatusCode.OK)
                    throw new Exception(createResult.Message);
            }
        }
        catch (Exception ex)
        {
            await notificationsService.AddAsync(
                new NotificationCreateModel
                {
                    Message = "Произошла внутренняя ошибка: " + ex.Message + "\n" + ex.InnerException?.Message,
                    SubscriberId = _adminId
                });
        }
    }
}