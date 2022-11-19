using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Schedule.Domain.DbModels;
using Schedule.Domain.Exceptions;
using Schedule.Domain.Models.CreateModels;
using Schedule.Services.Abstractions;
using Schedule.Services.HostedServices.BaseHostedServices;
using Schedule.Services.Utils;

namespace Schedule.Services.HostedServices.ScheduleHostedServices;

public class ScheduleParserHostedService : ScopedProcessor
{
    private readonly string _scheduleUrl;
    private readonly string _adminId;

    public ScheduleParserHostedService(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        : base(serviceScopeFactory, TimeSpan.FromSeconds(20))
    {
        _scheduleUrl = configuration["ScheduleUrl"];
        _adminId = configuration["AdminId"];
    }

    protected override async Task ProcessInScope(IServiceProvider scope)
    {
        var notificationService = scope.GetRequiredService<IBaseService<DbNotification>>();

        try
        {
            var parsingQueueService = scope.GetRequiredService<IParsingQueueService>();
            var scheduleService = scope.GetRequiredService<IScheduleService>();

            var groups = await parsingQueueService.GetAllAsync();
            if (groups.StatusCode != HttpStatusCode.OK)
                throw new Exception("Не удалось получить очередь парсинга");

            foreach (var group in groups.Data!)
            {
                var parsedSchedule = await ScheduleParser.ParseAsync(group.GroupName, _scheduleUrl);
                var removeFromQueueResult = await parsingQueueService.RemoveAsync(group);
                if (removeFromQueueResult.StatusCode != HttpStatusCode.OK || !removeFromQueueResult.Data)
                    throw new Exception($"Не удалось удалить группу {group.GroupName} из очереди парсинга");

                if (!parsedSchedule.IsSuccess)
                {
                    if (parsedSchedule.Exception is not ParserExceptions)
                        throw new Exception("Parser error", parsedSchedule.Exception);

                    var res = HandleParserExceptionAsync(parsedSchedule.Exception, group.GroupName);
                    await notificationService.AddAsync(
                        new NotificationCreateModel
                        {
                            Message = res.Message,
                            SubscriberId = res.ToAdmin ? _adminId : group.SubscriberId ?? _adminId
                        });
                }
                else
                {
                    var addScheduleResult = await scheduleService.ParsedToDbAsync(parsedSchedule.Schedule);
                    if (addScheduleResult.StatusCode != HttpStatusCode.OK || !addScheduleResult.Data)
                        throw new Exception($"Не удалось добавить расписание в базу данных для группы {group.GroupName}");

                    if (!group.IsNotificationNeeded) continue;

                    if (group.SubscriberId is null)
                        throw new Exception("Не удалось отправить уведомление (не указан id подписчика)");

                    var result = await notificationService.AddAsync(
                        new NotificationCreateModel
                        {
                            Message = $"Группа {group.GroupName} успешно добавлена в базу данных",
                            SubscriberId = group.SubscriberId
                        });

                    if (result.StatusCode != HttpStatusCode.OK)
                        throw new Exception("Не удалось отправить уведомление");
                }
            }
        }
        catch (Exception ex)
        {
            await notificationService.AddAsync(
                new NotificationCreateModel
                {
                    Message = "Произошла внутренняя ошибка: " + ex.Message + "\n" + ex.InnerException?.Message,
                    SubscriberId = _adminId
                });
        }
    }

    private (string Message, bool ToAdmin) HandleParserExceptionAsync(Exception exception, string groupName) =>
        exception switch
        {
            ParserExceptions.IncorrectGroupExceptions => ($"Группа {groupName} не найдена", false),
            ParserExceptions.CellNotFoundExceptions => ($"Не удалось найти группу {groupName} в Excel таблице", true),
            _ => ($"Неизвестная ошибка, группа {groupName}", true)
        };
}