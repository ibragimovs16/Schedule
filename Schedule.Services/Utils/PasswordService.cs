using System.Security.Cryptography;
using System.Text;

namespace Schedule.Services.Utils;

public static class PasswordService
{
    public record HashData(byte[]? Salt, byte[]? Hash);
    
    public static async Task<HashData> CreatePasswordHash(string password)
    {
        using var hmac = new HMACSHA512();
        return new HashData(
            hmac.Key,
            await hmac.ComputeHashAsync(new MemoryStream(Encoding.UTF8.GetBytes(password)))
        );
    }
    
    public static async Task<bool> VerifyPassword(string password, HashData passwordHashRecord)
    {
        if (passwordHashRecord.Hash is null || passwordHashRecord.Salt is null)
            return false;
        
        using var hmac = new HMACSHA512(passwordHashRecord.Salt);
        var computedHash = await hmac.ComputeHashAsync(new MemoryStream(Encoding.UTF8.GetBytes(password)));
        return computedHash.SequenceEqual(passwordHashRecord.Hash);
    }
}