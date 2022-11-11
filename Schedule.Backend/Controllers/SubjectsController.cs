using Microsoft.AspNetCore.Mvc;
using Schedule.Backend.Controllers.BaseControllers;
using Schedule.Domain.DbModels;
using Schedule.Domain.Models;
using Schedule.Services.Abstractions;

namespace Schedule.Backend.Controllers;

[ApiController]
public class SubjectsController : BaseApiController<DbSubject, ModelWithName, ModelWithName>
{
    public SubjectsController(IBaseService<DbSubject> service) : base(service)
    {
    }
}