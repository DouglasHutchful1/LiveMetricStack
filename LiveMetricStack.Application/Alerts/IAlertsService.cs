namespace LiveMetricStack.Application.Alerts;

public interface IAlertsService
{
    Task<AlertDto?> CreateAsync(CreateAlertRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AlertDto>> QueryAsync(AlertsQuery query, CancellationToken cancellationToken);
    Task<AlertDto?> ResolveAsync(Guid alertId, CancellationToken cancellationToken);
}
