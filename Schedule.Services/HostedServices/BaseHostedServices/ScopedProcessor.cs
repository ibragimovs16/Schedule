using Microsoft.Extensions.DependencyInjection;

namespace Schedule.Services.HostedServices.BaseHostedServices;

public abstract class ScopedProcessor : HostedServiceBase
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    protected ScopedProcessor(IServiceScopeFactory serviceScopeFactory, TimeSpan interval) : base(interval)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task Process()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        await ProcessInScope(scope.ServiceProvider);
    }

    protected abstract Task ProcessInScope(IServiceProvider scope);
}