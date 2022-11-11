using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Schedule.Domain.Models;
using Schedule.Domain.Responses;

namespace Schedule.Services.Abstractions;

public interface IAuthService
{
    Task<BaseResponse<string>> Register(User user);
    Task<BaseResponse<string>> Login(User user, IConfiguration configuration, IResponseCookies responseCookies);
    Task<BaseResponse<string>> RefreshToken(IRequestCookieCollection requestCookies, IResponseCookies responseCookies);
}