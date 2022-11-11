namespace Schedule.Domain.DbModels;

public class DbBellSchedule : DbEntity
{
    public int PairNumber { get; set; }
    public string Start { get; set; } = string.Empty;
    public string End { get; set; } = string.Empty;
}