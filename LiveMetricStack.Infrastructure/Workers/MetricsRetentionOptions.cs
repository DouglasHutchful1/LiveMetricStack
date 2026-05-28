namespace LiveMetricStack.Infrastructure.Workers;

public class MetricsRetentionOptions
{
    public const string SectionName = "MetricsRetention";

    public bool Enabled { get; set; } = true;
    public int RetentionDays { get; set; } = 30;
    public int IntervalMinutes { get; set; } = 60;
}
