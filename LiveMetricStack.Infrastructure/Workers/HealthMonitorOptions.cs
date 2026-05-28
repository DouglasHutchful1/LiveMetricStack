namespace LiveMetricStack.Infrastructure.Workers;

public class HealthMonitorOptions
{
    public const string SectionName = "HealthMonitor";

    public bool Enabled { get; set; } = true;
    public int IntervalSeconds { get; set; } = 30;
    public int TimeoutSeconds { get; set; } = 5;
    public List<HealthMonitorTarget> Targets { get; set; } = [];
}

public class HealthMonitorTarget
{
    public Guid ApplicationId { get; set; }
    public string Endpoint { get; set; } = string.Empty;
}
