namespace Schedule.Domain.DbModels;

public class DbNotification : DbEntity
{
    public bool HasBeenSent { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public string SubscriberId { get; set; } = string.Empty;
    public DateTime CreatedDateTime { get; set; } = DateTime.Now;
    public DateTime? SentDateTime { get; set; } = null;
}