using System.Text;
using LiveMetricStack.Application.Applications;
using LiveMetricStack.Application.Auth;
using LiveMetricStack.Application.Metrics;
using LiveMetricStack.Infrastructure.Auth;
using LiveMetricStack.Infrastructure.Persistence;
using LiveMetricStack.Infrastructure.Services;
using LiveMetricStack.Infrastructure.Workers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace LiveMetricStack.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<PasswordHasher>();
        services.AddScoped<JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IApplicationsService, ApplicationsService>();
        services.AddScoped<IMetricsService, MetricsService>();
        services.Configure<MetricsGeneratorOptions>(configuration.GetSection(MetricsGeneratorOptions.SectionName));
        services.AddHostedService<FakeMetricsWorker>();

        var connectionString = configuration.GetConnectionString("PostgreSql")
                               ?? throw new InvalidOperationException("Connection string 'PostgreSql' is missing.");

        services.AddDbContext<LiveMetricDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                         ?? throw new InvalidOperationException("JWT settings are missing.");

        if (string.IsNullOrWhiteSpace(jwtOptions.Key) || jwtOptions.Key.Length < 32)
        {
            throw new InvalidOperationException("JWT key must be at least 32 characters.");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }
}
