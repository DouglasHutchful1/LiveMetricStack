namespace LiveMetricStack.Application.Applications;

public class CreateApplicationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string? Status { get; set; }
}
