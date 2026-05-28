namespace LiveMetricStack.Application.Events;

public class EventDto
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
