using LiveMetricStack.Application.Metrics;
using LiveMetricStack.WebApi.Contracts.Metrics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IngestMetricContract = LiveMetricStack.WebApi.Contracts.Metrics.IngestMetricRequest;

namespace LiveMetricStack.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/metrics")]
public class MetricsController(IMetricsService metricsService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<MetricDto>> Ingest(IngestMetricContract request, CancellationToken cancellationToken)
    {
        var metric = await metricsService.IngestAsync(new LiveMetricStack.Application.Metrics.IngestMetricRequest
        {
            ApplicationId = request.ApplicationId,
            MetricName = request.MetricName,
            MetricValue = request.MetricValue,
            Unit = request.Unit,
            RecordedAtUtc = request.RecordedAtUtc
        }, cancellationToken);

        if (metric is null)
        {
            return NotFound(new { message = "Application not found." });
        }

        return Ok(metric);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<MetricDto>>> Query([FromQuery] MetricsQueryRequest request, CancellationToken cancellationToken)
    {
        var metrics = await metricsService.QueryAsync(new MetricsQuery
        {
            ApplicationId = request.ApplicationId,
            MetricName = request.MetricName,
            FromUtc = request.FromUtc,
            ToUtc = request.ToUtc,
            Take = request.Take
        }, cancellationToken);

        return Ok(metrics);
    }

    [HttpGet("{applicationId:guid}/latest")]
    public async Task<ActionResult<IReadOnlyCollection<MetricSnapshotDto>>> Latest(Guid applicationId, CancellationToken cancellationToken)
    {
        var snapshot = await metricsService.GetLatestSnapshotAsync(applicationId, cancellationToken);
        return Ok(snapshot);
    }
}
