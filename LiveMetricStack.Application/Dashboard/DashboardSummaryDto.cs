namespace LiveMetricStack.Application.Dashboard;

public class DashboardSummaryDto
{
    public DateTime GeneratedAtUtc { get; set; }
    public int TotalApplications { get; set; }
    public int OnlineApplications { get; set; }
    public int ActiveAlerts { get; set; }
    public IReadOnlyCollection<ApplicationSummaryDto> Applications { get; set; } = [];
}
