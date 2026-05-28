using LiveMetricStack.Application.Events;
using LiveMetricStack.WebApi.Contracts.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CreateEventContract = LiveMetricStack.WebApi.Contracts.Events.CreateEventRequest;

namespace LiveMetricStack.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/events")]
public class EventsController(IEventsService eventsService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<EventDto>> Create(CreateEventContract request, CancellationToken cancellationToken)
    {
        var evt = await eventsService.CreateAsync(new LiveMetricStack.Application.Events.CreateEventRequest
        {
            ApplicationId = request.ApplicationId,
            EventType = request.EventType,
            Message = request.Message,
            Severity = request.Severity
        }, cancellationToken);

        if (evt is null)
        {
            return NotFound(new { message = "Application not found." });
        }

        return Ok(evt);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<EventDto>>> Query([FromQuery] EventsQueryRequest request, CancellationToken cancellationToken)
    {
        var events = await eventsService.QueryAsync(new EventsQuery
        {
            ApplicationId = request.ApplicationId,
            Severity = request.Severity,
            Take = request.Take
        }, cancellationToken);

        return Ok(events);
    }
}
