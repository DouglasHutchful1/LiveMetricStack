using System.ComponentModel.DataAnnotations;

namespace LiveMetricStack.WebApi.Contracts.Alerts;

public class CreateAlertRequest
{
    [Required]
    public Guid ApplicationId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Message { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Severity { get; set; } = "Warning";
}
