namespace LiveMetricStack.Infrastructure.Workers;

public class MetricsGeneratorOptions
{
    public const string SectionName = "MetricsGenerator";

    public bool Enabled { get; set; } = true;
    public int IntervalSeconds { get; set; } = 5;
}
