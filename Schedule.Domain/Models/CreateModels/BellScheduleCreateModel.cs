namespace Schedule.Domain.Models.CreateModels;

public class BellScheduleCreateModel
{
    public int PairNumber { get; set; }
    public string Start { get; set; } = string.Empty;
    public string End { get; set; } = string.Empty;
}