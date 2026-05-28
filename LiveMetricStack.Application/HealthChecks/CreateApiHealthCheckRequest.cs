namespace LiveMetricStack.Application.HealthChecks;

public class CreateApiHealthCheckRequest
{
    public Guid ApplicationId { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public int? StatusCode { get; set; }
    public int? ResponseTimeMs { get; set; }
    public bool IsHealthy { get; set; }
    public DateTime? CheckedAtUtc { get; set; }
}
