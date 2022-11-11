namespace Schedule.Domain.Models.Params;

public class ScheduleParams : BaseParams
{
    public string Date { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public int PairNumber { get; set; } = -1;
}