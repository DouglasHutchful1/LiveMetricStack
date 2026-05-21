namespace LiveMetricStack.Application.Metrics;

public class IngestMetricRequest
{
    public Guid ApplicationId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public decimal MetricValue { get; set; }
    public string? Unit { get; set; }
    public DateTime? RecordedAtUtc { get; set; }
}
