namespace LiveMetricStack.Application.Alerts;

public class AlertsQuery
{
    public Guid? ApplicationId { get; set; }
    public bool? IsResolved { get; set; }
    public string? Severity { get; set; }
    public int Take { get; set; } = 200;
}
