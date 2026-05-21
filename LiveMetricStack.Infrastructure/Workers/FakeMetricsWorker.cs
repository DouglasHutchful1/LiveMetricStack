using LiveMetricStack.Domain.Entities;
using LiveMetricStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ApplicationEntity = LiveMetricStack.Domain.Entities.Application;

namespace LiveMetricStack.Infrastructure.Workers;

public class FakeMetricsWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<MetricsGeneratorOptions> options,
    ILogger<FakeMetricsWorker> logger) : BackgroundService
{
    private readonly MetricsGeneratorOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("FakeMetricsWorker is disabled by configuration.");
            return;
        }

        var intervalSeconds = Math.Max(_options.IntervalSeconds, 1);
        logger.LogInformation("FakeMetricsWorker started with {IntervalSeconds}s interval.", intervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<LiveMetricDbContext>();

                var applications = await dbContext.Applications.ToListAsync(stoppingToken);
                if (applications.Count == 0)
                {
                    var demoApp = new ApplicationEntity
                    {
                        Id = Guid.NewGuid(),
                        Name = "Demo API",
                        Environment = "Production",
                        ApiKey = $"lms_{Guid.NewGuid():N}",
                        Status = "Online",
                        CreatedAt = DateTime.UtcNow
                    };

                    dbContext.Applications.Add(demoApp);
                    await dbContext.SaveChangesAsync(stoppingToken);
                    applications.Add(demoApp);
                }

                var metrics = new List<Metric>(applications.Count * 4);
                foreach (var application in applications)
                {
                    metrics.Add(new Metric
                    {
                        Id = Guid.NewGuid(),
                        ApplicationId = application.Id,
                        MetricName = "cpu_usage",
                        MetricValue = Math.Round((decimal)(Random.Shared.NextDouble() * 100), 2),
                        Unit = "%",
                        RecordedAt = DateTime.UtcNow
                    });

                    metrics.Add(new Metric
                    {
                        Id = Guid.NewGuid(),
                        ApplicationId = application.Id,
                        MetricName = "memory_usage",
                        MetricValue = Math.Round((decimal)(300 + Random.Shared.NextDouble() * 3500), 2),
                        Unit = "MB",
                        RecordedAt = DateTime.UtcNow
                    });

                    metrics.Add(new Metric
                    {
                        Id = Guid.NewGuid(),
                        ApplicationId = application.Id,
                        MetricName = "request_count",
                        MetricValue = Random.Shared.Next(40, 1500),
                        Unit = "count",
                        RecordedAt = DateTime.UtcNow
                    });

                    metrics.Add(new Metric
                    {
                        Id = Guid.NewGuid(),
                        ApplicationId = application.Id,
                        MetricName = "error_rate",
                        MetricValue = Math.Round((decimal)(Random.Shared.NextDouble() * 7), 2),
                        Unit = "%",
                        RecordedAt = DateTime.UtcNow
                    });
                }

                dbContext.Metrics.AddRange(metrics);
                await dbContext.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "FakeMetricsWorker failed to generate metrics.");
            }

            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
        }
    }
}
