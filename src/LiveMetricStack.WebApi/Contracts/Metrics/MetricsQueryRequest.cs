namespace LiveMetricStack.WebApi.Contracts.Metrics;

public class MetricsQueryRequest
{
    public Guid? ApplicationId { get; set; }
    public string? MetricName { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public int Take { get; set; } = 200;
}
