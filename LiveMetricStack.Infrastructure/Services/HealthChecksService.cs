using LiveMetricStack.Application.HealthChecks;
using LiveMetricStack.Domain.Entities;
using LiveMetricStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LiveMetricStack.Infrastructure.Services;

public class HealthChecksService(LiveMetricDbContext dbContext) : IHealthChecksService
{
    public async Task<ApiHealthCheckDto?> CreateAsync(CreateApiHealthCheckRequest request, CancellationToken cancellationToken)
    {
        var appExists = await dbContext.Applications.AnyAsync(x => x.Id == request.ApplicationId, cancellationToken);
        if (!appExists)
        {
            return null;
        }

        var healthCheck = new ApiHealthCheck
        {
            Id = Guid.NewGuid(),
            ApplicationId = request.ApplicationId,
            Endpoint = request.Endpoint.Trim(),
            StatusCode = request.StatusCode,
            ResponseTimeMs = request.ResponseTimeMs,
            IsHealthy = request.IsHealthy,
            CheckedAt = request.CheckedAtUtc ?? DateTime.UtcNow
        };

        dbContext.ApiHealthChecks.Add(healthCheck);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(healthCheck);
    }

    public async Task<IReadOnlyCollection<ApiHealthCheckDto>> QueryAsync(HealthChecksQuery query, CancellationToken cancellationToken)
    {
        var take = query.Take <= 0 ? 200 : Math.Min(query.Take, 2000);

        var queryable = dbContext.ApiHealthChecks.AsNoTracking().AsQueryable();

        if (query.ApplicationId.HasValue)
        {
            queryable = queryable.Where(x => x.ApplicationId == query.ApplicationId.Value);
        }

        if (query.IsHealthy.HasValue)
        {
            queryable = queryable.Where(x => x.IsHealthy == query.IsHealthy.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Endpoint))
        {
            var endpoint = query.Endpoint.Trim();
            queryable = queryable.Where(x => x.Endpoint == endpoint);
        }

        return await queryable
            .OrderByDescending(x => x.CheckedAt)
            .Take(take)
            .Select(x => new ApiHealthCheckDto
            {
                Id = x.Id,
                ApplicationId = x.ApplicationId,
                Endpoint = x.Endpoint,
                StatusCode = x.StatusCode,
                ResponseTimeMs = x.ResponseTimeMs,
                IsHealthy = x.IsHealthy,
                CheckedAtUtc = x.CheckedAt
            })
            .ToListAsync(cancellationToken);
    }

    private static ApiHealthCheckDto Map(ApiHealthCheck healthCheck)
    {
        return new ApiHealthCheckDto
        {
            Id = healthCheck.Id,
            ApplicationId = healthCheck.ApplicationId,
            Endpoint = healthCheck.Endpoint,
            StatusCode = healthCheck.StatusCode,
            ResponseTimeMs = healthCheck.ResponseTimeMs,
            IsHealthy = healthCheck.IsHealthy,
            CheckedAtUtc = healthCheck.CheckedAt
        };
    }
}
