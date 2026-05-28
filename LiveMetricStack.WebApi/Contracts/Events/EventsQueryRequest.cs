namespace LiveMetricStack.WebApi.Contracts.Events;

public class EventsQueryRequest
{
    public Guid? ApplicationId { get; set; }
    public string? Severity { get; set; }
    public int Take { get; set; } = 200;
}
