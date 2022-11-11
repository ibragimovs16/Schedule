using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Schedule.Domain.DbModels;
using Schedule.Services.Abstractions;
using Schedule.Services.HostedServices.BaseHostedServices;

namespace Schedule.Services.HostedServices.ScheduleHostedServices;

public class ScheduleUpdaterHostedService : ScheduledProcessor
{
    public ScheduleUpdaterHostedService(IServiceScopeFactory serviceScopeFactory) 
        : base(serviceScopeFactory, TimeSpan.FromSeconds(20), 
            TimeSpan.FromSeconds(20))
    {
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
                    throw new Exception($"Не удалось добавить группу в очередь: {result.Message}");
            }
        }
        catch (Exception e)
        {
            // ToDo: Add notification about error to admin
            
            Console.WriteLine(e);
        }
    }

    protected override string Schedule => "0 0 * * *";
}