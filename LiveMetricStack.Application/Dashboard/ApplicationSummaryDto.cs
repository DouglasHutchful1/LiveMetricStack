namespace LiveMetricStack.Application.Dashboard;

public class ApplicationSummaryDto
{
    public Guid ApplicationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public decimal? CpuUsage { get; set; }
    public decimal? MemoryUsageMb { get; set; }
    public decimal? RequestCount { get; set; }
    public decimal? ErrorRate { get; set; }
    public DateTime? LastMetricAtUtc { get; set; }

    public int ActiveAlerts { get; set; }
}
