using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedule.Domain.DbModels;
using Schedule.Domain.Models.Params;
using Schedule.Services.Abstractions;

namespace Schedule.Backend.Controllers.BaseControllers;

/// <summary>
/// Base controller for all controllers
/// </summary>
[ApiController]
[Route("Api/[controller]")]
public abstract class BaseApiController<T, TC, TU> : ControllerBase, IDisposable
    where T : DbEntity, new() // Entity
    where TC : class // Create model
    where TU : class // Update model
{
    protected readonly IBaseService<T> Service;

    protected BaseApiController(IBaseService<T> service)
    {
        Service = service;
    }
    
    /// <summary>
    /// Получить список всех сущностей
    /// </summary>
    [HttpGet]
    public virtual async Task<IActionResult> GetAll()
    {
        var result = await Service.GetAllAsync();
        return Ok(result);
    }
    
    /// <summary>
    /// Получить сущность по идентификатору
    /// </summary>
    /// <param name="id">Идентификатору сущности</param>
    [HttpGet, Route("{id}")]
    public virtual async Task<IActionResult> Get(string id)
    {
        var result = await Service
            .FindByAsync(item => item.Id == id);
        return Ok(result);
    }
    
    /// <summary>
    /// Добавить модель
    /// </summary>
    [HttpPost, Authorize(Roles = "Admin")]
    public virtual async Task<IActionResult> Add([FromBody] TC model)
    {
        var result = await Service.AddAsync(model);
        return Ok(result);
    }
    
    /// <summary>
    /// Обновить данные
    /// </summary>
    [HttpPut, Route("{id}"), Authorize(Roles = "Admin")]
    public virtual async Task<IActionResult> Update([FromBody] TU model, string id)
    {
        var result = await Service.UpdateAsync(model, id);
        return Ok(result);
    }
    
    /// <summary>
    /// Удалить данные
    /// </summary>
    [HttpDelete, Route("{id}"), Authorize(Roles = "Admin")]
    public virtual async Task<IActionResult> Remove(string id)
    {
        var result = await Service.RemoveAsync(id);
        return Ok(result);
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
                Service.Dispose();
            }
        }

        _disposed = true;
    }

    #endregion
}