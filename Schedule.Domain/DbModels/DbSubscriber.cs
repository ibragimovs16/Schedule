namespace Schedule.Domain.DbModels;

public class DbSubscriber : DbEntity
{
    public long? TgChatId { get; set; }
    public long? TgUserId { get; set; }
}