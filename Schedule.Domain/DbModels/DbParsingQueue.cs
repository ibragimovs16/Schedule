namespace Schedule.Domain.DbModels;

public class DbParsingQueue : DbEntity
{
    public string GroupName { get; set; } = string.Empty;
    public bool IsUpdating { get; set; } = false;
    public bool IsNotificationNeeded { get; set; } = false;
    public string? SubscriberId { get; set; }
}