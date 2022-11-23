using Schedule.Domain.DbModels.DbEnums;

namespace Schedule.Domain.Models.CreateModels;

public class NotificationCreateModel
{
    public bool HasBeenSent { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public string SubscriberId { get; set; } = string.Empty;
    public DateTime CreatedDateTime { get; set; } = DateTime.Now;
    public DateTime? SentDateTime { get; set; } = null;
    public NotificationsTypes Type { get; set; }
}