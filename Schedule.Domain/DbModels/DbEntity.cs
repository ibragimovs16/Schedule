namespace Schedule.Domain.DbModels;

public abstract class DbEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
}