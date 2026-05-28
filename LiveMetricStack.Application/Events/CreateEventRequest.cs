namespace LiveMetricStack.Application.Events;

public class CreateEventRequest
{
    public Guid ApplicationId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "Info";
}
