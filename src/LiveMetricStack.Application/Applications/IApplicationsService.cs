namespace LiveMetricStack.Application.Applications;

public interface IApplicationsService
{
    Task<IReadOnlyCollection<ApplicationDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<ApplicationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ApplicationDto> CreateAsync(CreateApplicationRequest request, CancellationToken cancellationToken);
    Task<ApplicationDto?> UpdateStatusAsync(Guid id, UpdateApplicationStatusRequest request, CancellationToken cancellationToken);
}
