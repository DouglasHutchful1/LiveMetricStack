namespace LiveMetricStack.Application.HealthChecks;

public class HealthChecksQuery
{
    public Guid? ApplicationId { get; set; }
    public bool? IsHealthy { get; set; }
    public string? Endpoint { get; set; }
    public int Take { get; set; } = 200;
}
