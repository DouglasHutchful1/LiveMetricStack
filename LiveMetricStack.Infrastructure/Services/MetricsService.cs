using LiveMetricStack.Application.Metrics;
using LiveMetricStack.Domain.Entities;
using LiveMetricStack.Infrastructure.Caching;
using LiveMetricStack.Infrastructure.Persistence;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace LiveMetricStack.Infrastructure.Services;

public class MetricsService(
    LiveMetricDbContext dbContext,
    IDistributedCache cache,
    IOptions<CacheOptions> cacheOptions) : IMetricsService
{
    private readonly CacheOptions _cacheOptions = cacheOptions.Value;

    public async Task<MetricDto?> IngestAsync(IngestMetricRequest request, CancellationToken cancellationToken)
    {
        var appExists = await dbContext.Applications.AnyAsync(x => x.Id == request.ApplicationId, cancellationToken);
        if (!appExists)
        {
            return null;
        }

        var metric = new Metric
        {
            Id = Guid.NewGuid(),
            ApplicationId = request.ApplicationId,
            MetricName = request.MetricName.Trim(),
            MetricValue = request.MetricValue,
            Unit = request.Unit?.Trim(),
            RecordedAt = request.RecordedAtUtc ?? DateTime.UtcNow
        };

        dbContext.Metrics.Add(metric);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(metric);
    }

    public async Task<IReadOnlyCollection<MetricDto>> QueryAsync(MetricsQuery query, CancellationToken cancellationToken)
    {
        var take = query.Take <= 0 ? 200 : Math.Min(query.Take, 2000);
        var cacheKey = BuildQueryCacheKey(query, take);

        if (_cacheOptions.MetricsTtlSeconds > 0)
        {
            var cached = await cache.GetRecordAsync<List<MetricDto>>(cacheKey, cancellationToken);
            if (cached is not null)
            {
                return cached;
            }
        }

        var queryable = dbContext.Metrics.AsNoTracking().AsQueryable();

        if (query.ApplicationId.HasValue)
        {
            queryable = queryable.Where(x => x.ApplicationId == query.ApplicationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.MetricName))
        {
            var metricName = query.MetricName.Trim();
            queryable = queryable.Where(x => x.MetricName == metricName);
        }

        if (query.FromUtc.HasValue)
        {
            queryable = queryable.Where(x => x.RecordedAt >= query.FromUtc.Value);
        }

        if (query.ToUtc.HasValue)
        {
            queryable = queryable.Where(x => x.RecordedAt <= query.ToUtc.Value);
        }

        var result = await queryable
            .OrderByDescending(x => x.RecordedAt)
            .Take(take)
            .Select(x => new MetricDto
            {
                Id = x.Id,
                ApplicationId = x.ApplicationId,
                MetricName = x.MetricName,
                MetricValue = x.MetricValue,
                Unit = x.Unit,
                RecordedAtUtc = x.RecordedAt
            })
            .ToListAsync(cancellationToken);

        if (_cacheOptions.MetricsTtlSeconds > 0)
        {
            await cache.SetRecordAsync(
                cacheKey,
                result,
                TimeSpan.FromSeconds(_cacheOptions.MetricsTtlSeconds),
                cancellationToken);
        }

        return result;
    }

    public async Task<IReadOnlyCollection<MetricSnapshotDto>> GetLatestSnapshotAsync(Guid applicationId, CancellationToken cancellationToken)
    {
        var cacheKey = $"metrics:latest:{applicationId:D}";

        if (_cacheOptions.MetricsTtlSeconds > 0)
        {
            var cached = await cache.GetRecordAsync<List<MetricSnapshotDto>>(cacheKey, cancellationToken);
            if (cached is not null)
            {
                return cached;
            }
        }

        var query = dbContext.Metrics
            .AsNoTracking()
            .Where(x => x.ApplicationId == applicationId)
            .GroupBy(x => x.MetricName)
            .Select(group => group
                .OrderByDescending(x => x.RecordedAt)
                .Select(x => new MetricSnapshotDto
                {
                    ApplicationId = x.ApplicationId,
                    MetricName = x.MetricName,
                    MetricValue = x.MetricValue,
                    Unit = x.Unit,
                    RecordedAtUtc = x.RecordedAt
                })
                .First());

        var result = await query.ToListAsync(cancellationToken);

        if (_cacheOptions.MetricsTtlSeconds > 0)
        {
            await cache.SetRecordAsync(
                cacheKey,
                result,
                TimeSpan.FromSeconds(_cacheOptions.MetricsTtlSeconds),
                cancellationToken);
        }

        return result;
    }

    private static MetricDto Map(Metric metric)
    {
        return new MetricDto
        {
            Id = metric.Id,
            ApplicationId = metric.ApplicationId,
            MetricName = metric.MetricName,
            MetricValue = metric.MetricValue,
            Unit = metric.Unit,
            RecordedAtUtc = metric.RecordedAt
        };
    }

    private static string BuildQueryCacheKey(MetricsQuery query, int take)
    {
        return string.Join('|',
            "metrics:query",
            query.ApplicationId?.ToString("D") ?? "all",
            query.MetricName?.Trim() ?? "all",
            query.FromUtc?.ToString("O") ?? "none",
            query.ToUtc?.ToString("O") ?? "none",
            take.ToString());
    }
}
