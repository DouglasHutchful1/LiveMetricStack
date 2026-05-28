using LiveMetricStack.Application.Alerts;
using LiveMetricStack.WebApi.Contracts.Alerts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CreateAlertContract = LiveMetricStack.WebApi.Contracts.Alerts.CreateAlertRequest;

namespace LiveMetricStack.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/alerts")]
public class AlertsController(IAlertsService alertsService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AlertDto>> Create(CreateAlertContract request, CancellationToken cancellationToken)
    {
        var alert = await alertsService.CreateAsync(new LiveMetricStack.Application.Alerts.CreateAlertRequest
        {
            ApplicationId = request.ApplicationId,
            Title = request.Title,
            Message = request.Message,
            Severity = request.Severity
        }, cancellationToken);

        if (alert is null)
        {
            return NotFound(new { message = "Application not found." });
        }

        return Ok(alert);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<AlertDto>>> Query([FromQuery] AlertsQueryRequest request, CancellationToken cancellationToken)
    {
        var alerts = await alertsService.QueryAsync(new AlertsQuery
        {
            ApplicationId = request.ApplicationId,
            IsResolved = request.IsResolved,
            Severity = request.Severity,
            Take = request.Take
        }, cancellationToken);

        return Ok(alerts);
    }

    [HttpPatch("{id:guid}/resolve")]
    public async Task<ActionResult<AlertDto>> Resolve(Guid id, CancellationToken cancellationToken)
    {
        var alert = await alertsService.ResolveAsync(id, cancellationToken);
        if (alert is null)
        {
            return NotFound(new { message = "Alert not found." });
        }

        return Ok(alert);
    }
}
