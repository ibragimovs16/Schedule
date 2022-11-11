using Microsoft.Extensions.DependencyInjection;
using NCrontab;

namespace Schedule.Services.HostedServices.BaseHostedServices;

public abstract class ScheduledProcessor : ScopedProcessor
{
    private readonly CrontabSchedule _schedule;
    private readonly TimeSpan _scheduledInterval;
    private DateTime _nextRun;
    
    protected abstract string Schedule { get; }
    
    protected ScheduledProcessor(IServiceScopeFactory serviceScopeFactory, TimeSpan hostedServiceInterval, 
        TimeSpan scheduledInterval)
        : base(serviceScopeFactory, hostedServiceInterval)
    {
        _scheduledInterval = scheduledInterval;
        // ReSharper disable once VirtualMemberCallInConstructor
        _schedule = CrontabSchedule.Parse(Schedule);
        _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        do
        {
            var now = DateTime.Now;
            if (now > _nextRun)
            {
                await Process();
                _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
            }
            await Task.Delay(_scheduledInterval, stoppingToken);
        } while (!stoppingToken.IsCancellationRequested);
    }
}