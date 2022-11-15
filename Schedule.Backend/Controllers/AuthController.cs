using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedule.Domain.Models;
using Schedule.Domain.Models.UpdateModels;
using Schedule.Domain.Responses;
using Schedule.Services.Abstractions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Schedule.Backend.Controllers;

[ApiController]
[Route("Api/[controller]")]
public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }
    
    /// <summary>
    /// Регистрация пользователя
    /// </summary>
    /// <param name="request">Данные пользователя</param>
    [HttpPost("Register")]
    public async Task<ActionResult> Register([FromBody] User request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new BaseResponse<string>
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = JsonSerializer.Serialize(ModelState)
            });
        
        var result = await _authService.Register(request);
        return Ok(result);
    }
    
    /// <summary>
    /// Авторизация пользователя
    /// </summary>
    /// <param name="request">Данные пользователя</param>
    [HttpPost("Login")]
    public async Task<ActionResult> Login([FromBody] User request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new BaseResponse<string>
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = JsonSerializer.Serialize(ModelState)
            });
        
        var result = await _authService.Login(request, _configuration, Response.Cookies);

        if (result.StatusCode != HttpStatusCode.OK)
            return BadRequest(result);

        var data = new TokenModel { AccessToken = result.Data!.AccessToken };
        if (Request.Headers["User-Agent"].ToString().Contains("ScheduleBots"))
            data.RefreshToken = result.Data.RefreshToken;
        
        return Ok(new BaseResponse<TokenModel>
        {
            StatusCode = HttpStatusCode.OK,
            Data = data
        });
    }

    /// <summary>
    /// Обновить токен
    /// </summary>
    [HttpPost("RefreshToken")]
    public async Task<ActionResult> RefreshToken()
    {
        var result = await _authService.RefreshToken(Request.Cookies, Response.Cookies);
        
        if (result.StatusCode != HttpStatusCode.OK)
            return BadRequest(result);
        
        return Ok(new BaseResponse<Dictionary<string, string>>
        {
            StatusCode = HttpStatusCode.OK,
            Data = new Dictionary<string, string>{{"AccessToken", result.Data!}}
        });
    }
    
    /// <summary>
    /// Провека авторизации пользователя
    /// </summary>
    /// <returns></returns>
    [HttpGet("WhoAmI"), Authorize]
    public IActionResult WhoAmI()
    {
        return Ok(new BaseResponse<Dictionary<string, string>>
        {
            StatusCode = HttpStatusCode.OK,
            Data = new Dictionary<string, string>{{"Username", User.Identity!.Name!}}
        });
    }
}