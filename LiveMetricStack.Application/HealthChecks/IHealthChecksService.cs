namespace LiveMetricStack.Application.HealthChecks;

public interface IHealthChecksService
{
    Task<ApiHealthCheckDto?> CreateAsync(CreateApiHealthCheckRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ApiHealthCheckDto>> QueryAsync(HealthChecksQuery query, CancellationToken cancellationToken);
}
