namespace LiveMetricStack.Application.Alerts;

public class CreateAlertRequest
{
    public Guid ApplicationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "Warning";
}
