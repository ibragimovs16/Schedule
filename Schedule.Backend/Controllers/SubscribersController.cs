using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedule.Backend.Controllers.BaseControllers;
using Schedule.Domain.DbModels;
using Schedule.Domain.Models.CreateModels;
using Schedule.Services.Abstractions;

namespace Schedule.Backend.Controllers;

public class SubscribersController : BaseApiController<DbSubscribers, SubscribersCreateModel, SubscribersCreateModel>
{
    public SubscribersController(IBaseService<DbSubscribers> service) : base(service)
    {
    }
    
    [HttpGet, Authorize(Roles = "Admin")]
    public override Task<IActionResult> GetAll()
    {
        return base.GetAll();
    }
    
    [HttpGet, Route("{id}"), Authorize(Roles = "Admin")]
    public override Task<IActionResult> Get(string id)
    {
        return base.Get(id);
    }
}