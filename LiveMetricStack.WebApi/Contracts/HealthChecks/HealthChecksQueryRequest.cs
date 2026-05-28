namespace LiveMetricStack.WebApi.Contracts.HealthChecks;

public class HealthChecksQueryRequest
{
    public Guid? ApplicationId { get; set; }
    public bool? IsHealthy { get; set; }
    public string? Endpoint { get; set; }
    public int Take { get; set; } = 200;
}
