using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedule.Backend.Controllers.BaseControllers;
using Schedule.Domain.DbModels;
using Schedule.Domain.Models.CreateModels;
using Schedule.Domain.Models.UpdateModels;
using Schedule.Services.Abstractions;


namespace Schedule.Backend.Controllers;

[ApiController]
public class BellsScheduleController : BaseApiController<DbBellSchedule, BellScheduleCreateModel, BellScheduleUpdateModel>
{
    public BellsScheduleController(IBaseService<DbBellSchedule> service) : base(service)
    {
    }
    
    /// <summary>
    /// Получить время начала и конца пары по номеру пары
    /// </summary>
    /// <param name="pairNumber"></param>
    [HttpGet, Route("{pairNumber:int}")]
    public async Task<IActionResult> Get(int pairNumber)
    {
        var result = await Service
            .FindByAsync(bs => bs.PairNumber == pairNumber);
        return Ok(result);
    }
}