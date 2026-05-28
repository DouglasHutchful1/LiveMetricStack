using System.ComponentModel.DataAnnotations;

namespace LiveMetricStack.WebApi.Contracts.HealthChecks;

public class CreateHealthCheckRequest
{
    [Required]
    public Guid ApplicationId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Endpoint { get; set; } = string.Empty;

    public int? StatusCode { get; set; }

    public int? ResponseTimeMs { get; set; }

    public bool IsHealthy { get; set; }

    public DateTime? CheckedAtUtc { get; set; }
}
