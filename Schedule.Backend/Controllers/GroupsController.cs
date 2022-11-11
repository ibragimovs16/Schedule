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
}