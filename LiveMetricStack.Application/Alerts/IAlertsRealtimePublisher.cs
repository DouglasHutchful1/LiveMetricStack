namespace LiveMetricStack.Application.Alerts;

public interface IAlertsRealtimePublisher
{
    Task PublishCreatedAsync(AlertDto alert, CancellationToken cancellationToken);
    Task PublishResolvedAsync(AlertDto alert, CancellationToken cancellationToken);
}
