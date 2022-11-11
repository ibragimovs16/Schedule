using Schedule.Domain.Models;

namespace Schedule.Domain.Exceptions;

public class NotificationsSettingsExceptions : Exception
{
    private NotificationsSettingsExceptions(string message) : base(message)
    {
    }
    
    public class GroupNotFoundException : NotificationsSettingsExceptions
    {
        public GroupNotFoundException(string message) : base(message)
        {
        }
    }
    
    public class SubscriberNotFoundException : NotificationsSettingsExceptions
    {
        public SubscriberNotFoundException(string message) : base(message)
        {
        }
    }
    
    public class NotificationsSettingsNotFoundException : NotificationsSettingsExceptions
    {
        public NotificationsSettingsNotFoundException(string message) : base(message)
        {
        }
    }
}