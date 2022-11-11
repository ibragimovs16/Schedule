using Schedule.Domain.Models.Params;
using Schedule.Domain.Responses;
using Schedule.Services.Utils;

namespace Schedule.Services.Abstractions;

public interface IScheduleService : IDisposable
{
    Task<BaseResponse<List<Domain.Models.JoinedSchedule>>> GetAllAsync(ScheduleParams? pars = null);
    Task<BaseResponse<bool>> ParsedToDbAsync(ScheduleParser.Schedule schedule);
}