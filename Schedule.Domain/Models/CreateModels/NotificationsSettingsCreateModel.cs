using System.ComponentModel.DataAnnotations;

namespace Schedule.Domain.Models.CreateModels;

public class NotificationsSettingsCreateModel
{
    [Required]
    public string GroupName { get; set; } = string.Empty;
    public byte? DayOfWeek { get; set; }
    public string? Time { get; set; }
    
    [Required]
    public bool IsScheduleNotifyNeeded { get; set; }
    
    [Required]
    public bool IsEnabled { get; set; }
    
    [Required]
    public string SubscriberId { get; set; } = string.Empty;
}