using System.Data;
using Microsoft.EntityFrameworkCore;
using Schedule.Domain.DbModels;

namespace Schedule.DAL.Implementations;

public class NotificationsSettingsRepository : BaseRepository<DbNotificationsSetting>
{
    public NotificationsSettingsRepository(ApplicationDbContext db) : base(db)
    {
    }

    public override async Task<DbNotificationsSetting> AddAsync(DbNotificationsSetting entity)
    {
        var dbEntity = await Db.NotificationsSettings
            .FirstOrDefaultAsync(ns => ns.SubscriberId == entity.SubscriberId);

        if (dbEntity is null) return await base.AddAsync(entity);

        var group = await Db.Groups.FirstOrDefaultAsync(g => g.Id == dbEntity!.GroupId);
        if (dbEntity is not null)
            throw new Exception($"Вы уже подписаны на уведомления группы {group!.Name}");

        return await base.AddAsync(entity);
    }
}