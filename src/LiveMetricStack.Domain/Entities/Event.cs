namespace LiveMetricStack.Domain.Entities;

public class Event
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Application? Application { get; set; }
}
