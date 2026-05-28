using LiveMetricStack.Application.Alerts;
using LiveMetricStack.Domain.Entities;
using LiveMetricStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LiveMetricStack.Infrastructure.Services;

public class AlertsService(
    LiveMetricDbContext dbContext,
    IAlertsRealtimePublisher alertsRealtimePublisher) : IAlertsService
{
    public async Task<AlertDto?> CreateAsync(CreateAlertRequest request, CancellationToken cancellationToken)
    {
        var appExists = await dbContext.Applications.AnyAsync(x => x.Id == request.ApplicationId, cancellationToken);
        if (!appExists)
        {
            return null;
        }

        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            ApplicationId = request.ApplicationId,
            Title = request.Title.Trim(),
            Message = request.Message.Trim(),
            Severity = string.IsNullOrWhiteSpace(request.Severity) ? "Warning" : request.Severity.Trim(),
            IsResolved = false,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Alerts.Add(alert);
        await dbContext.SaveChangesAsync(cancellationToken);

        var result = Map(alert);
        await alertsRealtimePublisher.PublishCreatedAsync(result, cancellationToken);
        return result;
    }

    public async Task<IReadOnlyCollection<AlertDto>> QueryAsync(AlertsQuery query, CancellationToken cancellationToken)
    {
        var take = query.Take <= 0 ? 200 : Math.Min(query.Take, 2000);

        var queryable = dbContext.Alerts.AsNoTracking().AsQueryable();

        if (query.ApplicationId.HasValue)
        {
            queryable = queryable.Where(x => x.ApplicationId == query.ApplicationId.Value);
        }

        if (query.IsResolved.HasValue)
        {
            queryable = queryable.Where(x => x.IsResolved == query.IsResolved.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Severity))
        {
            var severity = query.Severity.Trim();
            queryable = queryable.Where(x => x.Severity == severity);
        }

        return await queryable
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .Select(x => new AlertDto
            {
                Id = x.Id,
                ApplicationId = x.ApplicationId,
                Title = x.Title,
                Message = x.Message,
                Severity = x.Severity,
                IsResolved = x.IsResolved,
                CreatedAtUtc = x.CreatedAt,
                ResolvedAtUtc = x.ResolvedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AlertDto?> ResolveAsync(Guid alertId, CancellationToken cancellationToken)
    {
        var alert = await dbContext.Alerts.FirstOrDefaultAsync(x => x.Id == alertId, cancellationToken);
        if (alert is null)
        {
            return null;
        }

        if (!alert.IsResolved)
        {
            alert.IsResolved = true;
            alert.ResolvedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var result = Map(alert);
        await alertsRealtimePublisher.PublishResolvedAsync(result, cancellationToken);
        return result;
    }

    private static AlertDto Map(Alert alert)
    {
        return new AlertDto
        {
            Id = alert.Id,
            ApplicationId = alert.ApplicationId,
            Title = alert.Title,
            Message = alert.Message,
            Severity = alert.Severity,
            IsResolved = alert.IsResolved,
            CreatedAtUtc = alert.CreatedAt,
            ResolvedAtUtc = alert.ResolvedAt
        };
    }
}
