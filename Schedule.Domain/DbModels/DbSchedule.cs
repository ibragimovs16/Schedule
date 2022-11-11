namespace Schedule.Domain.DbModels;

public class DbSchedule : DbEntity
{
    public string Group { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string BellSchedule { get; set; } = string.Empty;
}