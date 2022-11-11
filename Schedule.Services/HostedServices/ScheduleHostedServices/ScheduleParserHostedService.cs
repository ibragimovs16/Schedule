using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Schedule.Domain.Exceptions;
using Schedule.Services.Abstractions;
using Schedule.Services.HostedServices.BaseHostedServices;
using Schedule.Services.Utils;

namespace Schedule.Services.HostedServices.ScheduleHostedServices;

public class ScheduleParserHostedService : ScopedProcessor
{
    private readonly string _scheduleUrl;

    public ScheduleParserHostedService(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        : base(serviceScopeFactory, TimeSpan.FromSeconds(20))
    {
        _scheduleUrl = configuration["ScheduleUrl"];
    }

    protected override async Task ProcessInScope(IServiceProvider scope)
    {
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
                    throw new Exception("Не удалось удалить группу из очереди парсинга");

                if (!parsedSchedule.IsSuccess)
                {
                    if (parsedSchedule.Exception is not ParserExceptions)
                        throw new Exception("Parser error", parsedSchedule.Exception);

                    await HandleParserExceptionAsync(parsedSchedule.Exception);
                }
                else
                {
                    var addScheduleResult = await scheduleService.ParsedToDbAsync(parsedSchedule.Schedule);
                    if (addScheduleResult.StatusCode != HttpStatusCode.OK || !addScheduleResult.Data)
                        throw new Exception("Не удалось добавить расписание в базу данных");

                    // ToDo: Add notification when success
                    Console.WriteLine(group.IsNotificationNeeded);

                    Console.WriteLine(group.IsUpdating
                        ? $"Данные длы группы {group.GroupName} обновлены"
                        : $"Группа {group.GroupName} успешно добавлена в базу данных");
                }
            }
        }
        catch (Exception ex)
        {
            // ToDo: Add notification when error

            Console.WriteLine("Произошла внутренняя ошибка: " + ex.Message + "\n" + ex.InnerException?.Message);
        }
    }
    
    private Task HandleParserExceptionAsync(Exception exception)
    {
        switch (exception)
        {
            case ParserExceptions.IncorrectGroupExceptions:
                Console.WriteLine("Группа не найдена");
                break;
            case ParserExceptions.CellNotFoundExceptions:
                Console.WriteLine("Не удалось найти группу в Excel таблице");
                break;
        }

        return Task.CompletedTask;
    }
}