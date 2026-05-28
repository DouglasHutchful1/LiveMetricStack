using LiveMetricStack.Application.HealthChecks;
using LiveMetricStack.WebApi.Contracts.HealthChecks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CreateHealthCheckContract = LiveMetricStack.WebApi.Contracts.HealthChecks.CreateHealthCheckRequest;

namespace LiveMetricStack.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/health-checks")]
public class HealthChecksController(IHealthChecksService healthChecksService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiHealthCheckDto>> Create(CreateHealthCheckContract request, CancellationToken cancellationToken)
    {
        var healthCheck = await healthChecksService.CreateAsync(new CreateApiHealthCheckRequest
        {
            ApplicationId = request.ApplicationId,
            Endpoint = request.Endpoint,
            StatusCode = request.StatusCode,
            ResponseTimeMs = request.ResponseTimeMs,
            IsHealthy = request.IsHealthy,
            CheckedAtUtc = request.CheckedAtUtc
        }, cancellationToken);

        if (healthCheck is null)
        {
            return NotFound(new { message = "Application not found." });
        }

        return Ok(healthCheck);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ApiHealthCheckDto>>> Query([FromQuery] HealthChecksQueryRequest request, CancellationToken cancellationToken)
    {
        var checks = await healthChecksService.QueryAsync(new HealthChecksQuery
        {
            ApplicationId = request.ApplicationId,
            IsHealthy = request.IsHealthy,
            Endpoint = request.Endpoint,
            Take = request.Take
        }, cancellationToken);

        return Ok(checks);
    }
}
