using LiveMetricStack.Application.Dashboard;
using LiveMetricStack.Infrastructure.Caching;
using LiveMetricStack.Infrastructure.Persistence;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace LiveMetricStack.Infrastructure.Services;

public class DashboardService(
    LiveMetricDbContext dbContext,
    IDistributedCache cache,
    IOptions<CacheOptions> cacheOptions) : IDashboardService
{
    private readonly CacheOptions _cacheOptions = cacheOptions.Value;

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken)
    {
        const string cacheKey = "dashboard:summary";

        if (_cacheOptions.DashboardTtlSeconds > 0)
        {
            var cached = await cache.GetRecordAsync<DashboardSummaryDto>(cacheKey, cancellationToken);
            if (cached is not null)
            {
                return cached;
            }
        }

        var apps = await dbContext.Applications
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Environment,
                x.Status
            })
            .ToListAsync(cancellationToken);

        var activeAlertsByApp = await dbContext.Alerts
            .AsNoTracking()
            .Where(x => !x.IsResolved)
            .GroupBy(x => x.ApplicationId)
            .Select(g => new { ApplicationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ApplicationId, x => x.Count, cancellationToken);

        var latestMetrics = await dbContext.Metrics
            .AsNoTracking()
            .GroupBy(x => new { x.ApplicationId, x.MetricName })
            .Select(g => g
                .OrderByDescending(x => x.RecordedAt)
                .Select(x => new
                {
                    x.ApplicationId,
                    x.MetricName,
                    x.MetricValue,
                    x.RecordedAt
                })
                .First())
            .ToListAsync(cancellationToken);

        var metricsByApp = latestMetrics
            .GroupBy(x => x.ApplicationId)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(k => k.MetricName, v => (v.MetricValue, v.RecordedAt), StringComparer.OrdinalIgnoreCase));

        var applicationCards = apps.Select(app =>
        {
            metricsByApp.TryGetValue(app.Id, out var appMetrics);
            activeAlertsByApp.TryGetValue(app.Id, out var activeAlerts);

            return new ApplicationSummaryDto
            {
                ApplicationId = app.Id,
                Name = app.Name,
                Environment = app.Environment,
                Status = app.Status,
                CpuUsage = GetMetricValue(appMetrics, "cpu_usage"),
                MemoryUsageMb = GetMetricValue(appMetrics, "memory_usage"),
                RequestCount = GetMetricValue(appMetrics, "request_count"),
                ErrorRate = GetMetricValue(appMetrics, "error_rate"),
                LastMetricAtUtc = GetLastMetricTime(appMetrics),
                ActiveAlerts = activeAlerts
            };
        }).ToList();

        var summary = new DashboardSummaryDto
        {
            GeneratedAtUtc = DateTime.UtcNow,
            TotalApplications = applicationCards.Count,
            OnlineApplications = applicationCards.Count(x => x.Status.Equals("Online", StringComparison.OrdinalIgnoreCase)),
            ActiveAlerts = applicationCards.Sum(x => x.ActiveAlerts),
            Applications = applicationCards
        };

        if (_cacheOptions.DashboardTtlSeconds > 0)
        {
            await cache.SetRecordAsync(
                cacheKey,
                summary,
                TimeSpan.FromSeconds(_cacheOptions.DashboardTtlSeconds),
                cancellationToken);
        }

        return summary;
    }

    private static decimal? GetMetricValue(
        Dictionary<string, (decimal MetricValue, DateTime RecordedAt)>? metrics,
        string key)
    {
        if (metrics is null)
        {
            return null;
        }

        return metrics.TryGetValue(key, out var entry) ? entry.MetricValue : null;
    }

    private static DateTime? GetLastMetricTime(Dictionary<string, (decimal MetricValue, DateTime RecordedAt)>? metrics)
    {
        if (metrics is null || metrics.Count == 0)
        {
            return null;
        }

        return metrics.Values.Max(x => x.RecordedAt);
    }
}
