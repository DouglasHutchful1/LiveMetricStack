using System.ComponentModel.DataAnnotations;

namespace LiveMetricStack.WebApi.Contracts.Applications;

public class CreateApplicationRequest
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Environment { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Status { get; set; }
}
