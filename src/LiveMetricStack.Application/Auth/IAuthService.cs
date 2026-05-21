namespace LiveMetricStack.Application.Auth;

public interface IAuthService
{
    Task<AuthResult?> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken);
    Task<AuthResult?> LoginAsync(LoginUserRequest request, CancellationToken cancellationToken);
    Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken);
}
