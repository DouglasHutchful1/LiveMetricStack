using System.ComponentModel.DataAnnotations;

namespace LiveMetricStack.WebApi.Contracts.Applications;

public class UpdateApplicationStatusRequest
{
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;
}
