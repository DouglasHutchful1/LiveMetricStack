namespace LiveMetricStack.Domain.Entities;

public class Application
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Status { get; set; } = "Online";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Metric> Metrics { get; set; } = new List<Metric>();
    public ICollection<Event> Events { get; set; } = new List<Event>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    public ICollection<ApiHealthCheck> ApiHealthChecks { get; set; } = new List<ApiHealthCheck>();
}
