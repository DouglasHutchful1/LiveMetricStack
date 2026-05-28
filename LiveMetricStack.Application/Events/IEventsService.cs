namespace LiveMetricStack.Application.Events;

public interface IEventsService
{
    Task<EventDto?> CreateAsync(CreateEventRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<EventDto>> QueryAsync(EventsQuery query, CancellationToken cancellationToken);
}
