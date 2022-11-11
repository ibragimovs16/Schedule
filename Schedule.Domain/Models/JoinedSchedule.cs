namespace Schedule.Domain.Models;

public class JoinedSchedule
{
    public string GroupName { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public int PairNumber { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Start { get; set; } = string.Empty;
    public string End { get; set; } = string.Empty;
}