using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Schedule.Domain.DbModels;
using Schedule.Domain.Models;

namespace Schedule.Services.Utils;

public static class TokenService
{
    public static string CreateToken(DbUser user, IConfiguration configuration)
    {
        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var securityKey = configuration.GetSection("JWT:SecurityKey").Value;
        var lifeTimeInDays = configuration.GetSection("JWT:LifeTimeInDays").Value;

        if (securityKey is null || lifeTimeInDays is null)
            throw new Exception("Security key or lifetime not configured in appsettings.json");
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var token = new JwtSecurityToken(
            claims: claims,
            signingCredentials: cred,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddDays(int.Parse(lifeTimeInDays))
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public static RefreshToken GenerateRefreshToken(IConfiguration configuration)
    {
        var lifeTimeInDays = configuration.GetSection("JWT:RefreshTokenLifeTimeInDays").Value;
        
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Expires = DateTime.UtcNow.AddDays(int.Parse(lifeTimeInDays)),
            Created = DateTime.UtcNow
        };

        return refreshToken;
    }
}