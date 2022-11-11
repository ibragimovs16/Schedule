using Schedule.Domain.Enums;

namespace Schedule.Domain.DbModels;

public class DbUser : DbEntity
{
    public string Username { get; set; } = string.Empty;
    public byte[]? PasswordHash { get; set; }
    public byte[]? PasswordSalt { get; set; }
    public DateTime Registered { get; set; }
    public Roles Role { get; set; }
    public string? RefreshToken { get; set; } = string.Empty;
    public DateTime? RefreshTokenCreated { get; set; }
    public DateTime? RefreshTokenExpires { get; set; }
}