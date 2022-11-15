using Microsoft.EntityFrameworkCore;
using Schedule.Domain.DbModels;
using Schedule.Domain.Models;

namespace Schedule.DAL;

public class ApplicationDbContext : DbContext
{
#pragma warning disable CS8618
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {

    }
#pragma warning restore CS8618
    
    public DbSet<DbBellSchedule> BellsSchedule { get; set; }
    public DbSet<DbUser> Users { get; set; }
    public DbSet<DbGroup> Groups { get; set; }
    public DbSet<DbSubject> Subjects { get; set; }
    public DbSet<DbDate> Dates { get; set; }
    public DbSet<DbSchedule> Schedule { get; set; }
    public DbSet<DbParsingQueue> ParsingQueue { get; set; }
    public DbSet<DbNotificationsSetting> NotificationsSettings { get; set; }
    public DbSet<DbSubscriber> Subscribers { get; set; }
    public DbSet<DbNotification> Notifications { get; set; }
}