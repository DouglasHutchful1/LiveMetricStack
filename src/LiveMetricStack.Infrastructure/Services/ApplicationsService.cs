using LiveMetricStack.Application.Applications;
using LiveMetricStack.Domain.Entities;
using LiveMetricStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ApplicationEntity = LiveMetricStack.Domain.Entities.Application;

namespace LiveMetricStack.Infrastructure.Services;

public class ApplicationsService(LiveMetricDbContext dbContext) : IApplicationsService
{
    public async Task<IReadOnlyCollection<ApplicationDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Applications
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ApplicationDto
            {
                Id = x.Id,
                Name = x.Name,
                Environment = x.Environment,
                ApiKey = x.ApiKey,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ApplicationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Applications
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ApplicationDto
            {
                Id = x.Id,
                Name = x.Name,
                Environment = x.Environment,
                ApiKey = x.ApiKey,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ApplicationDto> CreateAsync(CreateApplicationRequest request, CancellationToken cancellationToken)
    {
        var app = new ApplicationEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Environment = request.Environment.Trim(),
            ApiKey = $"lms_{Guid.NewGuid():N}",
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Online" : request.Status.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Applications.Add(app);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(app);
    }

    public async Task<ApplicationDto?> UpdateStatusAsync(Guid id, UpdateApplicationStatusRequest request, CancellationToken cancellationToken)
    {
        var app = await dbContext.Applications.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (app is null)
        {
            return null;
        }

        app.Status = request.Status.Trim();
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(app);
    }

    private static ApplicationDto Map(ApplicationEntity app)
    {
        return new ApplicationDto
        {
            Id = app.Id,
            Name = app.Name,
            Environment = app.Environment,
            ApiKey = app.ApiKey,
            Status = app.Status,
            CreatedAt = app.CreatedAt
        };
    }
}
