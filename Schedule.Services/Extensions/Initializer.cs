using Microsoft.Extensions.DependencyInjection;
using Schedule.DAL.Abstractions;
using Schedule.DAL.Implementations;
using Schedule.Domain.DbModels;
using Schedule.Services.Abstractions;
using Schedule.Services.HostedServices.ScheduleHostedServices;
using Schedule.Services.Implementations;

namespace Schedule.Services.Extensions;

public static class Initializer
{
    public static void InitializeRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        services.AddScoped<IParsingQueueRepository, ParsingQueueRepository>();
    }
    
    public static void InitializeServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(IBaseService<DbNotificationsSettings>), typeof(NotificationsSettingsService));
        services.AddScoped(typeof(IBaseService<>), typeof(BaseService<>));
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<IParsingQueueService, ParsingQueueService>();
    }
    
    public static void InitializeHostedServices(this IServiceCollection services)
    {
        services.AddHostedService<ScheduleParserHostedService>();
        services.AddHostedService<ScheduleUpdaterHostedService>();
    }
}