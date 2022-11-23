using Schedule.Domain.DbModels.DbEnums;

namespace Schedule.Domain.Models;

public class NotifyModel
{
    public NotificationsTypes Type { get; set; } = NotificationsTypes.Message;
    public string Message { get; set; } = string.Empty;
    public long ChatId { get; set; }
    public long UserId { get; set; }
}