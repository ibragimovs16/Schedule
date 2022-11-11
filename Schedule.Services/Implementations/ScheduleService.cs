using System.Net;
using Microsoft.EntityFrameworkCore;
using Schedule.DAL.Abstractions;
using Schedule.Domain.DbModels;
using Schedule.Domain.Models;
using Schedule.Domain.Models.Params;
using Schedule.Domain.Responses;
using Schedule.Services.Abstractions;
using Schedule.Services.Utils;

namespace Schedule.Services.Implementations;

public class ScheduleService : IScheduleService
{
    private readonly IBaseRepository<DbBellSchedule> _bellScheduleRepository;
    private readonly IBaseRepository<DbDate> _datesRepository;
    private readonly IBaseRepository<DbGroup> _groupsRepository;
    private readonly IBaseRepository<DbSubject> _subjectsRepository;
    private readonly IBaseRepository<DbSchedule> _scheduleRepository;

    public ScheduleService(IBaseRepository<DbBellSchedule> bellScheduleRepository, IBaseRepository<DbDate> datesRepository, 
        IBaseRepository<DbGroup> groupsRepository, IBaseRepository<DbSubject> subjectsRepository, 
        IBaseRepository<DbSchedule> scheduleRepository)
    {
        _bellScheduleRepository = bellScheduleRepository;
        _datesRepository = datesRepository;
        _groupsRepository = groupsRepository;
        _subjectsRepository = subjectsRepository;
        _scheduleRepository = scheduleRepository;
    }

    public async Task<BaseResponse<List<JoinedSchedule>>> GetAllAsync(ScheduleParams? pars = null)
    {
        try
        {
            var scheduleQuery = pars?.Id is null
                ? _scheduleRepository.GetQuery()
                : _scheduleRepository.GetQuery(x => x.Id == pars.Id.ToString());
            
            var result = from schedule in scheduleQuery
                join dbGroup in _groupsRepository.GetQuery() on schedule.Group equals dbGroup.Id
                join dbDate in _datesRepository.GetQuery() on schedule.Date equals dbDate.Id
                join dbSubject in _subjectsRepository.GetQuery() on schedule.Subject equals dbSubject.Id
                join dbBellSchedule in _bellScheduleRepository.GetQuery() on schedule.BellSchedule equals dbBellSchedule
                    .Id
                orderby dbBellSchedule.PairNumber
                select new JoinedSchedule
                {
                    GroupName = dbGroup.Name,
                    Date = dbDate.Date,
                    Subject = dbSubject.Name,
                    PairNumber = dbBellSchedule.PairNumber,
                    Start = dbBellSchedule.Start,
                    End = dbBellSchedule.End
                };

            if (pars == null)
                return new BaseResponse<List<JoinedSchedule>>
                {
                    StatusCode = HttpStatusCode.OK,
                    Data = await result.ToListAsync()
                };

            if (!string.IsNullOrEmpty(pars.Date))
                result = result.Where(s => s.Date == pars.Date);
            if (!string.IsNullOrEmpty(pars.GroupName))
                result = result.Where(s => s.GroupName == pars.GroupName);
            if (pars.PairNumber != -1)
                result = result.Where(s => s.PairNumber == pars.PairNumber);

            return new BaseResponse<List<JoinedSchedule>>
            {
                StatusCode = HttpStatusCode.OK,
                Data = await result.ToListAsync()
            };
        }
        catch (Exception e)
        {
            return new BaseResponse<List<JoinedSchedule>>
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = e.Message
            };
        }
    }

    public async Task<BaseResponse<bool>> ParsedToDbAsync(ScheduleParser.Schedule schedule)
    {
        try
        {
            var dbGroups = await _groupsRepository.FindByAsync(g => g.Name == schedule.Group);
            var dbGroup = dbGroups.Count == 1 
                ? dbGroups.First() 
                : await _groupsRepository.AddAsync(new DbGroup {Name = schedule.Group});

            foreach (var groupData in schedule.Data)
            {
                var dbDates = await _datesRepository.FindByAsync(d => d.Date == groupData.Date);
                var dbDate = dbDates.Count == 1 
                    ? dbDates.First() 
                    : await _datesRepository.AddAsync(new DbDate {Date = groupData.Date});

                foreach (var subject in groupData.Subjects)
                {
                    var dbSubjects = await _subjectsRepository
                        .FindByAsync(s => s.Name == subject.Name);
                    var dbSubject = dbSubjects.Count == 1 
                        ? dbSubjects.First() 
                        : await _subjectsRepository.AddAsync(new DbSubject {Name = subject.Name});

                    var dbBellSchedules = await _bellScheduleRepository
                        .FindByAsync(b => b.PairNumber == subject.PairNumber);
                    if (dbBellSchedules.Count == 0)
                        throw new ArgumentException("Неверный номер пары");
                    
                    var dbSchedule = new DbSchedule
                    {
                        Group = dbGroup.Id,
                        Date = dbDate.Id,
                        Subject = dbSubject.Id,
                        BellSchedule = dbBellSchedules.First().Id
                    };

                    var dbSchedules = await _scheduleRepository
                        .FindByAsync(s => s.Group == dbSchedule.Group
                                          && s.Date == dbSchedule.Date
                                          && s.Subject == dbSchedule.Subject
                                          && s.BellSchedule == dbSchedule.BellSchedule);

                    if (dbSchedules.Count == 0)
                        await _scheduleRepository.AddAsync(dbSchedule);
                }
            }

            return new BaseResponse<bool>
            {
                StatusCode = HttpStatusCode.OK,
                Data = true
            };
        }
        catch (Exception e)
        {
            return new BaseResponse<bool>
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = e.Message
            };
        }
    }

    #region Dispose

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _bellScheduleRepository.Dispose();
                _datesRepository.Dispose();
                _groupsRepository.Dispose();
                _subjectsRepository.Dispose();
                _scheduleRepository.Dispose();
            }
        }

        _disposed = true;
    }

    #endregion
}