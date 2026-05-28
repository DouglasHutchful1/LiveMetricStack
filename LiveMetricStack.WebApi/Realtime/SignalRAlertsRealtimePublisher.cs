using LiveMetricStack.Application.Alerts;
using LiveMetricStack.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace LiveMetricStack.WebApi.Realtime;

public class SignalRAlertsRealtimePublisher(IHubContext<MetricsHub> hubContext) : IAlertsRealtimePublisher
{
    public Task PublishCreatedAsync(AlertDto alert, CancellationToken cancellationToken)
    {
        var groupName = MetricsHub.ApplicationGroup(alert.ApplicationId);
        return hubContext.Clients.Group(groupName).SendAsync("alerts:new", alert, cancellationToken);
    }

    public Task PublishResolvedAsync(AlertDto alert, CancellationToken cancellationToken)
    {
        var groupName = MetricsHub.ApplicationGroup(alert.ApplicationId);
        return hubContext.Clients.Group(groupName).SendAsync("alerts:resolved", alert, cancellationToken);
    }
}
