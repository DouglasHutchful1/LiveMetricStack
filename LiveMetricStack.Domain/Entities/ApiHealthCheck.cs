namespace LiveMetricStack.Domain.Entities;

public class ApiHealthCheck
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public int? StatusCode { get; set; }
    public int? ResponseTimeMs { get; set; }
    public bool IsHealthy { get; set; }
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

    public Application? Application { get; set; }
}
