using System.ComponentModel.DataAnnotations;

namespace LiveMetricStack.WebApi.Contracts.Events;

public class CreateEventRequest
{
    [Required]
    public Guid ApplicationId { get; set; }

    [Required]
    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Message { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Severity { get; set; } = "Info";
}
