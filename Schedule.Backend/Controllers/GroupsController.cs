using Microsoft.AspNetCore.Mvc;
using Schedule.Backend.Controllers.BaseControllers;
using Schedule.Domain.DbModels;
using Schedule.Domain.Models;
using Schedule.Services.Abstractions;

namespace Schedule.Backend.Controllers;

[ApiController]
public class GroupsController : BaseApiController<DbGroup, ModelWithName, ModelWithName>
{
    public GroupsController(IBaseService<DbGroup> service) : base(service)
    {
    }
    
    [HttpGet, Route("Search")]
    public async Task<IActionResult> Search(string groupName)
    {
        var result = await Service
            .FindByAsync(g => g.Name == groupName);

        return ActionResponse(result);
    }
}