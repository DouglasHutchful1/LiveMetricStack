namespace LiveMetricStack.Infrastructure.Caching;

public class CacheOptions
{
    public const string SectionName = "Cache";

    public bool EnableRedis { get; set; } = false;
    public int MetricsTtlSeconds { get; set; } = 10;
    public int DashboardTtlSeconds { get; set; } = 10;
}
