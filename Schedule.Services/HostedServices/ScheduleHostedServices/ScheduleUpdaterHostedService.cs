using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Schedule.Domain.DbModels;
using Schedule.Domain.Models.CreateModels;
using Schedule.Services.Abstractions;
using Schedule.Services.HostedServices.BaseHostedServices;

namespace Schedule.Services.HostedServices.ScheduleHostedServices;

public class ScheduleUpdaterHostedService : ScheduledProcessor
{
    private readonly string _adminId;
    
    public ScheduleUpdaterHostedService(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration) 
        : base(serviceScopeFactory, TimeSpan.FromSeconds(20), 
            TimeSpan.FromSeconds(20))
    {
        _adminId = configuration["AdminId"];
    }

    protected override async Task ProcessInScope(IServiceProvider scope)
    {
        try
        {
            var parsingQueueService = scope.GetRequiredService<IParsingQueueService>();
            var groupsService = scope.GetRequiredService<IBaseService<DbGroup>>();
            
            var groups = await groupsService.GetAllAsync();
            if (groups.StatusCode != HttpStatusCode.OK)
                throw new Exception("Не удалось получить список групп");

            foreach (var group in groups.Data!)
            {
                var result = await parsingQueueService
                    .AddAsync(group.Name, false, null, true);
                if (result.StatusCode != HttpStatusCode.OK)
                    throw new Exception($"Не удалось добавить группу {group.Name} в очередь: {result.Message}");
            }
        }
        catch (Exception ex)
        {
            var notificationService = scope.GetRequiredService<IBaseService<DbNotification>>();
            await notificationService.AddAsync(
                new NotificationCreateModel
                {
                    Message = "Произошла внутренняя ошибка: " + ex.Message + "\n" + ex.InnerException?.Message,
                    SubscriberId = _adminId
                });
        }
    }

    protected override string Schedule => "0 0 * * *";
}