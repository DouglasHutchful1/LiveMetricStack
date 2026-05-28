using LiveMetricStack.Application.Metrics;
using LiveMetricStack.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace LiveMetricStack.WebApi.Realtime;

public class SignalRMetricsRealtimePublisher(IHubContext<MetricsHub> hubContext) : IMetricsRealtimePublisher
{
    public async Task PublishAsync(IReadOnlyCollection<MetricDto> metrics, CancellationToken cancellationToken)
    {
        if (metrics.Count == 0)
        {
            return;
        }

        foreach (var applicationBatch in metrics.GroupBy(x => x.ApplicationId))
        {
            var groupName = MetricsHub.ApplicationGroup(applicationBatch.Key);
            await hubContext.Clients.Group(groupName)
                .SendAsync("metrics:batch", applicationBatch.ToList(), cancellationToken);
        }
    }
}
