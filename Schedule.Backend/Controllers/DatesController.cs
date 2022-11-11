using Microsoft.AspNetCore.Mvc;
using Schedule.Backend.Controllers.BaseControllers;
using Schedule.Domain.DbModels;
using Schedule.Domain.Models;
using Schedule.Services.Abstractions;

namespace Schedule.Backend.Controllers;

[ApiController]
public class DatesController : BaseApiController<DbDate, ModelWithName, ModelWithName>
{
    public DatesController(IBaseService<DbDate> service) : base(service)
    {
    }
}