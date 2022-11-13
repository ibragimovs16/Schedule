using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Schedule.Backend.Controllers.BaseControllers;
using Schedule.Domain.DbModels;
using Schedule.Domain.Models;
using Schedule.Domain.Models.Params;
using Schedule.Domain.Responses;
using Schedule.Services.Abstractions;

namespace Schedule.Backend.Controllers;

[ApiController]
public class ScheduleController : BaseApiController<DbSchedule, ScheduleModel, ScheduleModel>
{
    private readonly IScheduleService _scheduleService;
    private readonly IParsingQueueService _parsingQueueService;

    public ScheduleController(IBaseService<DbSchedule> service, IScheduleService scheduleService, IParsingQueueService parsingQueueService) : base(service)
    {
        _scheduleService = scheduleService;
        _parsingQueueService = parsingQueueService;
    }
    
    /// <summary>
    /// Получить все расписание
    /// </summary>
    public override async Task<IActionResult> GetAll()
    {
        var result = await _scheduleService.GetAllAsync();
        return Ok(result);
    }
    
    /// <summary>
    /// Получить расписание по параметрам
    /// </summary>
    /// <param name="scheduleParams"></param>
    /// <returns></returns>
    [HttpGet("GetByParams")]
    public async Task<IActionResult> GetByParams([FromQuery] ScheduleParams scheduleParams)
    {
        var result = await _scheduleService.GetAllAsync(scheduleParams);
        return Ok(result);
    }

    /// <summary>
    /// Запросить сбор данных по указанной группе, для добавления этих данных в БД
    /// </summary>
    /// <param name="groupName">Название группы</param>
    /// <param name="isNotificationNeeded">Отправить ли уведомление, когда закончится сбор данных
    /// (будет отправлено только если ранее были настроены уведомления)
    /// </param>
    /// <param name="subscriberId">Id подписчика</param>
    [HttpPost("RequestDataCollection")]
    public async Task<IActionResult> RequestDataCollection([Required] string groupName, bool isNotificationNeeded = false,
        string? subscriberId = null)
    {
        if (isNotificationNeeded && string.IsNullOrEmpty(subscriberId))
            return BadRequest(new BaseResponse<string>
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Не указан Id подписчика"
            });
        
        var result = await _parsingQueueService.AddAsync(groupName, isNotificationNeeded, subscriberId, false);
        if (result.StatusCode != HttpStatusCode.OK)
            return Conflict(result);
        
        return Ok(result);
    }
}