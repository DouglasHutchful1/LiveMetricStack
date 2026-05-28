using LiveMetricStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveMetricStack.Infrastructure.Workers;

public class MetricsRetentionWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<MetricsRetentionOptions> options,
    ILogger<MetricsRetentionWorker> logger) : BackgroundService
{
    private readonly MetricsRetentionOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("MetricsRetentionWorker is disabled by configuration.");
            return;
        }

        var retentionDays = Math.Max(_options.RetentionDays, 1);
        var interval = TimeSpan.FromMinutes(Math.Max(_options.IntervalMinutes, 1));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<LiveMetricDbContext>();

                var cutoff = DateTime.UtcNow.AddDays(-retentionDays);
                var deleted = await dbContext.Metrics
                    .Where(x => x.RecordedAt < cutoff)
                    .ExecuteDeleteAsync(stoppingToken);

                if (deleted > 0)
                {
                    logger.LogInformation("MetricsRetentionWorker deleted {DeletedCount} metrics older than {CutoffUtc}.", deleted, cutoff);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "MetricsRetentionWorker failed while pruning metrics.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}
