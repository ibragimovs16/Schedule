namespace Schedule.Domain.Models.Params;

public class BaseParams
{
    public Guid? Id { get; set; }
    public bool OrderDesc { get; set; } = false;
}