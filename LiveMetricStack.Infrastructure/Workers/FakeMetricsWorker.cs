using LiveMetricStack.Application.Alerts;
using LiveMetricStack.Domain.Entities;
using LiveMetricStack.Infrastructure.Persistence;
using LiveMetricStack.Application.Metrics;
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

                var highErrorAlerts = await dbContext.Alerts
                    .Where(x => !x.IsResolved && x.Title == "High Error Rate")
                    .GroupBy(x => x.ApplicationId)
                    .Select(group => group.OrderByDescending(x => x.CreatedAt).First())
                    .ToDictionaryAsync(x => x.ApplicationId, stoppingToken);

                var metrics = new List<Metric>(applications.Count * 4);
                var events = new List<Event>(applications.Count);
                var createdAlerts = new List<Alert>();
                var resolvedAlerts = new List<Alert>();

                foreach (var application in applications)
                {
                    var recordedAt = DateTime.UtcNow;
                    var cpuUsage = Math.Round((decimal)(Random.Shared.NextDouble() * 100), 2);
                    var memoryUsage = Math.Round((decimal)(300 + Random.Shared.NextDouble() * 3500), 2);
                    var requestCount = Random.Shared.Next(40, 1500);
                    var errorRate = Math.Round((decimal)(Random.Shared.NextDouble() * 7), 2);

                    metrics.Add(new Metric
                    {
                        Id = Guid.NewGuid(),
                        ApplicationId = application.Id,
                        MetricName = "cpu_usage",
                        MetricValue = cpuUsage,
                        Unit = "%",
                        RecordedAt = recordedAt
                    });

                    metrics.Add(new Metric
                    {
                        Id = Guid.NewGuid(),
                        ApplicationId = application.Id,
                        MetricName = "memory_usage",
                        MetricValue = memoryUsage,
                        Unit = "MB",
                        RecordedAt = recordedAt
                    });

                    metrics.Add(new Metric
                    {
                        Id = Guid.NewGuid(),
                        ApplicationId = application.Id,
                        MetricName = "request_count",
                        MetricValue = requestCount,
                        Unit = "count",
                        RecordedAt = recordedAt
                    });

                    metrics.Add(new Metric
                    {
                        Id = Guid.NewGuid(),
                        ApplicationId = application.Id,
                        MetricName = "error_rate",
                        MetricValue = errorRate,
                        Unit = "%",
                        RecordedAt = recordedAt
                    });

                    events.Add(new Event
                    {
                        Id = Guid.NewGuid(),
                        ApplicationId = application.Id,
                        EventType = "MetricSnapshotGenerated",
                        Message = $"Generated metrics snapshot. Error rate: {errorRate:0.00}%.",
                        Severity = "Info",
                        CreatedAt = recordedAt
                    });

                    if (errorRate >= 5)
                    {
                        events.Add(new Event
                        {
                            Id = Guid.NewGuid(),
                            ApplicationId = application.Id,
                            EventType = "HighErrorRate",
                            Message = $"High error rate detected ({errorRate:0.00}%).",
                            Severity = "Warning",
                            CreatedAt = recordedAt
                        });

                        if (!highErrorAlerts.ContainsKey(application.Id))
                        {
                            var createdAlert = new Alert
                            {
                                Id = Guid.NewGuid(),
                                ApplicationId = application.Id,
                                Title = "High Error Rate",
                                Message = $"Error rate reached {errorRate:0.00}%.",
                                Severity = "Critical",
                                IsResolved = false,
                                CreatedAt = recordedAt
                            };

                            dbContext.Alerts.Add(createdAlert);
                            createdAlerts.Add(createdAlert);
                            highErrorAlerts[application.Id] = createdAlert;
                        }
                    }
                    else if (errorRate <= 2 && highErrorAlerts.TryGetValue(application.Id, out var existingAlert))
                    {
                        existingAlert.IsResolved = true;
                        existingAlert.ResolvedAt = recordedAt;
                        resolvedAlerts.Add(existingAlert);
                        highErrorAlerts.Remove(application.Id);

                        events.Add(new Event
                        {
                            Id = Guid.NewGuid(),
                            ApplicationId = application.Id,
                            EventType = "HighErrorRateRecovered",
                            Message = $"Error rate recovered to {errorRate:0.00}%.",
                            Severity = "Info",
                            CreatedAt = recordedAt
                        });
                    }
                }

                dbContext.Metrics.AddRange(metrics);
                dbContext.Events.AddRange(events);
                await dbContext.SaveChangesAsync(stoppingToken);

                var metricsPublisher = scope.ServiceProvider.GetService<IMetricsRealtimePublisher>();
                if (metricsPublisher is not null)
                {
                    var batch = metrics.Select(x => new MetricDto
                    {
                        Id = x.Id,
                        ApplicationId = x.ApplicationId,
                        MetricName = x.MetricName,
                        MetricValue = x.MetricValue,
                        Unit = x.Unit,
                        RecordedAtUtc = x.RecordedAt
                    }).ToList();

                    await metricsPublisher.PublishAsync(batch, stoppingToken);
                }

                var alertsPublisher = scope.ServiceProvider.GetService<IAlertsRealtimePublisher>();
                if (alertsPublisher is not null)
                {
                    foreach (var createdAlert in createdAlerts)
                    {
                        await alertsPublisher.PublishCreatedAsync(ToAlertDto(createdAlert), stoppingToken);
                    }

                    foreach (var resolvedAlert in resolvedAlerts)
                    {
                        await alertsPublisher.PublishResolvedAsync(ToAlertDto(resolvedAlert), stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "FakeMetricsWorker failed to generate metrics.");
            }

            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
        }
    }

    private static AlertDto ToAlertDto(Alert alert)
    {
        return new AlertDto
        {
            Id = alert.Id,
            ApplicationId = alert.ApplicationId,
            Title = alert.Title,
            Message = alert.Message,
            Severity = alert.Severity,
            IsResolved = alert.IsResolved,
            CreatedAtUtc = alert.CreatedAt,
            ResolvedAtUtc = alert.ResolvedAt
        };
    }
}
