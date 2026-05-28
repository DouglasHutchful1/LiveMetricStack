namespace LiveMetricStack.Application.Events;

public class EventsQuery
{
    public Guid? ApplicationId { get; set; }
    public string? Severity { get; set; }
    public int Take { get; set; } = 200;
}
