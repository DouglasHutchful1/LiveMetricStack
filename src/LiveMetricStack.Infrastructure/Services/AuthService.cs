using LiveMetricStack.Application.Auth;
using LiveMetricStack.Domain.Entities;
using LiveMetricStack.Infrastructure.Auth;
using LiveMetricStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LiveMetricStack.Infrastructure.Services;

public class AuthService(
    LiveMetricDbContext dbContext,
    PasswordHasher passwordHasher,
    JwtTokenGenerator tokenGenerator,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<AuthResult?> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await UserExistsByEmailAsync(normalizedEmail, cancellationToken);
        if (exists)
        {
            return null;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return BuildAuthResult(user);
    }

    public async Task<AuthResult?> LoginAsync(LoginUserRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        return BuildAuthResult(user);
    }

    public Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken);
    }

    private AuthResult BuildAuthResult(User user)
    {
        return new AuthResult
        {
            AccessToken = tokenGenerator.Generate(user),
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes),
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
        };
    }
}
