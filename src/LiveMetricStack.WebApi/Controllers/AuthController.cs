using LiveMetricStack.Application.Auth;
using LiveMetricStack.WebApi.Contracts.Auth;
using Microsoft.AspNetCore.Mvc;

namespace LiveMetricStack.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResult>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var exists = await authService.UserExistsByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (exists)
        {
            return Conflict(new { message = "A user with this email already exists." });
        }

        var result = await authService.RegisterAsync(new RegisterUserRequest
        {
            FullName = request.FullName,
            Email = request.Email,
            Password = request.Password
        }, cancellationToken);

        if (result is null)
        {
            return Conflict(new { message = "A user with this email already exists." });
        }

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResult>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(new LoginUserRequest
        {
            Email = request.Email,
            Password = request.Password
        }, cancellationToken);

        if (result is null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(result);
    }
}
