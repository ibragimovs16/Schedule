using Schedule.Domain.DbModels;

namespace Schedule.DAL.Implementations;

public class NotificationsRepository : BaseRepository<DbNotification>
{
    public NotificationsRepository(ApplicationDbContext db) : base(db)
    {
    }

    public override async Task<DbNotification> AddAsync(DbNotification entity)
    {
        var notifications = Db.Notifications
            .Where(n => n.Message == entity.Message &&
                        n.SubscriberId == entity.SubscriberId &&
                        n.CreatedDateTime.Hour == entity.CreatedDateTime.Hour &&
                        n.CreatedDateTime.Minute == entity.CreatedDateTime.Minute);

        if (notifications.Any())
            return notifications.First();

        return await base.AddAsync(entity);
    }
}