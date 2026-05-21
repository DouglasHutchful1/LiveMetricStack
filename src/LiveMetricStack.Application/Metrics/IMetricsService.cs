namespace LiveMetricStack.Application.Metrics;

public interface IMetricsService
{
    Task<MetricDto?> IngestAsync(IngestMetricRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<MetricDto>> QueryAsync(MetricsQuery query, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<MetricSnapshotDto>> GetLatestSnapshotAsync(Guid applicationId, CancellationToken cancellationToken);
}
