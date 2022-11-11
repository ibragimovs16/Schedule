using Microsoft.Extensions.Hosting;

namespace Schedule.Services.HostedServices.BaseHostedServices;

public abstract class HostedServiceBase : IHostedService
{
    private Task? _executingTask;
    private readonly CancellationTokenSource _stoppingCts = new();
    private readonly TimeSpan _interval;

    protected HostedServiceBase(TimeSpan interval)
    {
        _interval = interval;
    }

    public virtual Task StartAsync(CancellationToken cancellationToken)
    {
        _executingTask = ExecuteAsync(_stoppingCts.Token);

        return _executingTask.IsCompleted 
            ? _executingTask 
            : Task.CompletedTask;
    }

    public virtual async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_executingTask is null) return;

        try
        {
            _stoppingCts.Cancel();
        }
        finally
        {
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
    }
    
    protected virtual async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        do
        {
            await Process();

            await Task.Delay(_interval, stoppingToken);
        }
        while (!stoppingToken.IsCancellationRequested);
    }

    protected abstract Task Process();
}