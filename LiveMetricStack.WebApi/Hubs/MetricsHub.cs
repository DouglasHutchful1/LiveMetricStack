using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LiveMetricStack.WebApi.Hubs;

[Authorize]
public class MetricsHub : Hub
{
    public static string ApplicationGroup(Guid applicationId) => $"app:{applicationId:D}";

    public Task JoinApplication(Guid applicationId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, ApplicationGroup(applicationId));
    }

    public Task LeaveApplication(Guid applicationId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, ApplicationGroup(applicationId));
    }
}
