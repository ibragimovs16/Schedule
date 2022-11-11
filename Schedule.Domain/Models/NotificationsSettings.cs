namespace Schedule.Domain.Models;

public class NotificationsSettings
{
    public string GroupName { get; set; } = string.Empty;
    public byte? DayOfWeek { get; set; }
    public string? Time { get; set; }
    public bool IsScheduleNotifyNeeded { get; set; }
    public bool IsEnabled { get; set; }
    public string? Email { get; set; }
}