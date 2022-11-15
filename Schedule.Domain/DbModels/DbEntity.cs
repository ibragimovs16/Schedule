using System.ComponentModel.DataAnnotations;

namespace Schedule.Domain.DbModels;

public abstract class DbEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
}