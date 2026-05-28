using System.Diagnostics;
using LiveMetricStack.Domain.Entities;
using LiveMetricStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveMetricStack.Infrastructure.Workers;

public class HealthMonitorWorker(
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    IOptions<HealthMonitorOptions> options,
    ILogger<HealthMonitorWorker> logger) : BackgroundService
{
    private readonly HealthMonitorOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("HealthMonitorWorker is disabled by configuration.");
            return;
        }

        if (_options.Targets.Count == 0)
        {
            logger.LogInformation("HealthMonitorWorker has no configured targets.");
            return;
        }

        var interval = TimeSpan.FromSeconds(Math.Max(_options.IntervalSeconds, 5));
        var timeoutSeconds = Math.Max(_options.TimeoutSeconds, 1);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<LiveMetricDbContext>();
                var client = httpClientFactory.CreateClient(nameof(HealthMonitorWorker));

                var appIds = await dbContext.Applications
                    .AsNoTracking()
                    .Select(x => x.Id)
                    .ToListAsync(stoppingToken);
                var validAppIds = appIds.ToHashSet();

                var checks = new List<ApiHealthCheck>();
                var events = new List<Event>();

                foreach (var target in _options.Targets)
                {
                    if (!validAppIds.Contains(target.ApplicationId) || string.IsNullOrWhiteSpace(target.Endpoint))
                    {
                        continue;
                    }

                    var stopwatch = Stopwatch.StartNew();
                    HttpResponseMessage? response = null;
                    Exception? error = null;

                    try
                    {
                        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                        timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
                        response = await client.GetAsync(target.Endpoint, timeoutCts.Token);
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                    }

                    stopwatch.Stop();

                    var isHealthy = response is { IsSuccessStatusCode: true };
                    int? statusCode = response is null ? null : (int)response.StatusCode;

                    checks.Add(new ApiHealthCheck
                    {
                        Id = Guid.NewGuid(),
                        ApplicationId = target.ApplicationId,
                        Endpoint = target.Endpoint,
                        StatusCode = statusCode,
                        ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                        IsHealthy = isHealthy,
                        CheckedAt = DateTime.UtcNow
                    });

                    if (!isHealthy)
                    {
                        var message = error is null
                            ? $"Health check failed for {target.Endpoint} (status {statusCode})."
                            : $"Health check failed for {target.Endpoint}: {error.Message}";

                        events.Add(new Event
                        {
                            Id = Guid.NewGuid(),
                            ApplicationId = target.ApplicationId,
                            EventType = "HealthCheckFailed",
                            Message = message,
                            Severity = "Error",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                if (checks.Count > 0)
                {
                    dbContext.ApiHealthChecks.AddRange(checks);
                }

                if (events.Count > 0)
                {
                    dbContext.Events.AddRange(events);
                }

                if (checks.Count > 0 || events.Count > 0)
                {
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "HealthMonitorWorker failed while running checks.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}
