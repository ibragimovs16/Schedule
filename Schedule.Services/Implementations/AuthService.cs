using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Schedule.DAL.Abstractions;
using Schedule.Domain.DbModels;
using Schedule.Domain.Enums;
using Schedule.Domain.Models;
using Schedule.Domain.Responses;
using Schedule.Services.Abstractions;
using Schedule.Services.Utils;

namespace Schedule.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IBaseRepository<DbUser> _repository;
    private readonly IConfiguration _configuration;

    public AuthService(IBaseRepository<DbUser> repository, IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    public async Task<BaseResponse<string>> Register(User user)
    {
        var usr = await _repository.FindByAsync(u => u.Username == user.Username);
        if (usr.Count > 0)
            return new BaseResponse<string>
            {
                StatusCode = HttpStatusCode.BadRequest,
                Data = "Указанное имя уже занято."
            };

        var pwd = await PasswordService.CreatePasswordHash(user.Password);

        var currentUser = new DbUser
        {
            Username = user.Username,
            PasswordHash = pwd.Hash,
            PasswordSalt = pwd.Salt,
            Registered = DateTime.UtcNow,
            Role = Roles.User
        };

        var entity = await _repository.AddAsync(currentUser);
        if (entity.Username != user.Username)
            return new BaseResponse<string>
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Data = "Что-то пошло не так, попробуйте позже."
            };

        return new BaseResponse<string>
        {
            StatusCode = HttpStatusCode.OK,
            Data = "You have successfully registered."
        };
    }

    public async Task<BaseResponse<string>> Login(User user, IConfiguration configuration, 
        IResponseCookies responseCookies)
    {
        var currentUser = await _repository.FindByAsync(u => u.Username == user.Username);

        if (currentUser.Count == 0 ||
            !await PasswordService.VerifyPassword(
                user.Password,
                new PasswordService.HashData(
                    currentUser.First().PasswordSalt,
                    currentUser.First().PasswordHash
                )))
            return new BaseResponse<string>
            {
                StatusCode = HttpStatusCode.BadRequest,
                Data = "Incorrect username or password."
            };
        
        var newRefreshToken = TokenService.GenerateRefreshToken(_configuration);
        await SetRefreshToken(currentUser.First(), newRefreshToken, responseCookies);
        
        return new BaseResponse<string>
        {
            StatusCode = HttpStatusCode.OK,
            Data = TokenService.CreateToken(currentUser.First(), configuration)
        };
    }

    public async Task<BaseResponse<string>> RefreshToken(IRequestCookieCollection requestCookies, 
        IResponseCookies responseCookies)
    {
        var refreshToken = requestCookies["refreshToken"];
        var result = await _repository
            .FindByAsync(u => u.RefreshToken == (refreshToken ?? string.Empty));

        if (result.Count == 0)
            return new BaseResponse<string>
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Data = "Invalid refresh token."
            };

        var currentUser = result.First();

        if (currentUser.RefreshTokenExpires < DateTime.UtcNow)
            return new BaseResponse<string>
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Data = "Refresh token expired."
            };

        var token = TokenService.CreateToken(currentUser, _configuration);
        var newRefreshToken = TokenService.GenerateRefreshToken(_configuration);
        await SetRefreshToken(currentUser, newRefreshToken, responseCookies);

        return new BaseResponse<string>
        {
            StatusCode = HttpStatusCode.OK,
            Data = token
        };
    }

    private async Task SetRefreshToken(DbUser currentUser, RefreshToken refreshToken, IResponseCookies responseCookies)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = refreshToken.Expires
        };
        
        responseCookies.Append("refreshToken", refreshToken.Token, cookieOptions);
        currentUser.RefreshToken = refreshToken.Token;
        currentUser.RefreshTokenCreated = refreshToken.Created;
        currentUser.RefreshTokenExpires = refreshToken.Expires;

        await _repository.UpdateAsync(currentUser);
    }
}