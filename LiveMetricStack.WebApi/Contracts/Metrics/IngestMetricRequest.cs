using System.ComponentModel.DataAnnotations;

namespace LiveMetricStack.WebApi.Contracts.Metrics;

public class IngestMetricRequest
{
    [Required]
    public Guid ApplicationId { get; set; }

    [Required]
    [MaxLength(100)]
    public string MetricName { get; set; } = string.Empty;

    [Range(typeof(decimal), "-1000000000", "1000000000")]
    public decimal MetricValue { get; set; }

    [MaxLength(50)]
    public string? Unit { get; set; }

    public DateTime? RecordedAtUtc { get; set; }
}
