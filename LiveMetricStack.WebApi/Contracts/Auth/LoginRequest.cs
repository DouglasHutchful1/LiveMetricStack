using System.ComponentModel.DataAnnotations;

namespace LiveMetricStack.WebApi.Contracts.Auth;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}
