namespace LiveMetricStack.Application.Metrics;

public interface IMetricsRealtimePublisher
{
    Task PublishAsync(IReadOnlyCollection<MetricDto> metrics, CancellationToken cancellationToken);
}
