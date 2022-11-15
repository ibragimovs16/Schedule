namespace Schedule.Domain.DbModels;

public class DbNotificationsSetting : DbEntity
{
    public string GroupId { get; set; } = string.Empty;
    public byte? DayOfWeek { get; set; }
    public string? Time { get; set; }
    public bool IsScheduleNotifyNeeded { get; set; }
    public bool IsEnabled { get; set; }
    public string SubscriberId { get; set; } = string.Empty;
}