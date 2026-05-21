namespace LiveMetricStack.Domain.Entities;

public class Metric
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public decimal MetricValue { get; set; }
    public string? Unit { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    public Application? Application { get; set; }
}
