using LiveMetricStack.Application.Events;
using LiveMetricStack.Domain.Entities;
using LiveMetricStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LiveMetricStack.Infrastructure.Services;

public class EventsService(LiveMetricDbContext dbContext) : IEventsService
{
    public async Task<EventDto?> CreateAsync(CreateEventRequest request, CancellationToken cancellationToken)
    {
        var appExists = await dbContext.Applications.AnyAsync(x => x.Id == request.ApplicationId, cancellationToken);
        if (!appExists)
        {
            return null;
        }

        var evt = new Event
        {
            Id = Guid.NewGuid(),
            ApplicationId = request.ApplicationId,
            EventType = request.EventType.Trim(),
            Message = request.Message.Trim(),
            Severity = string.IsNullOrWhiteSpace(request.Severity) ? "Info" : request.Severity.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Events.Add(evt);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(evt);
    }

    public async Task<IReadOnlyCollection<EventDto>> QueryAsync(EventsQuery query, CancellationToken cancellationToken)
    {
        var take = query.Take <= 0 ? 200 : Math.Min(query.Take, 2000);

        var queryable = dbContext.Events.AsNoTracking().AsQueryable();

        if (query.ApplicationId.HasValue)
        {
            queryable = queryable.Where(x => x.ApplicationId == query.ApplicationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Severity))
        {
            var severity = query.Severity.Trim();
            queryable = queryable.Where(x => x.Severity == severity);
        }

        return await queryable
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .Select(x => new EventDto
            {
                Id = x.Id,
                ApplicationId = x.ApplicationId,
                EventType = x.EventType,
                Message = x.Message,
                Severity = x.Severity,
                CreatedAtUtc = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    private static EventDto Map(Event evt)
    {
        return new EventDto
        {
            Id = evt.Id,
            ApplicationId = evt.ApplicationId,
            EventType = evt.EventType,
            Message = evt.Message,
            Severity = evt.Severity,
            CreatedAtUtc = evt.CreatedAt
        };
    }
}
