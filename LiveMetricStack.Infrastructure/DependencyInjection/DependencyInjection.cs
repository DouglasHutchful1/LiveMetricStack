using System.Text;
using LiveMetricStack.Application.Applications;
using LiveMetricStack.Application.Auth;
using LiveMetricStack.Application.Dashboard;
using LiveMetricStack.Application.Events;
using LiveMetricStack.Application.HealthChecks;
using LiveMetricStack.Application.Metrics;
using LiveMetricStack.Application.Alerts;
using LiveMetricStack.Infrastructure.Auth;
using LiveMetricStack.Infrastructure.Caching;
using LiveMetricStack.Infrastructure.Persistence;
using LiveMetricStack.Infrastructure.Services;
using LiveMetricStack.Infrastructure.Workers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace LiveMetricStack.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));

        services.AddScoped<PasswordHasher>();
        services.AddScoped<JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IApplicationsService, ApplicationsService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IEventsService, EventsService>();
        services.AddScoped<IAlertsService, AlertsService>();
        services.AddScoped<IHealthChecksService, HealthChecksService>();
        services.AddScoped<IMetricsService, MetricsService>();

        services.AddHttpClient(nameof(HealthMonitorWorker));

        var cacheOptions = configuration.GetSection(CacheOptions.SectionName).Get<CacheOptions>() ?? new CacheOptions();
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (cacheOptions.EnableRedis && !string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "lms:";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.Configure<MetricsGeneratorOptions>(configuration.GetSection(MetricsGeneratorOptions.SectionName));
        services.Configure<MetricsRetentionOptions>(configuration.GetSection(MetricsRetentionOptions.SectionName));
        services.Configure<HealthMonitorOptions>(configuration.GetSection(HealthMonitorOptions.SectionName));
        services.AddHostedService<FakeMetricsWorker>();
        services.AddHostedService<MetricsRetentionWorker>();
        services.AddHostedService<HealthMonitorWorker>();

        var connectionStringRaw = configuration.GetConnectionString("PostgreSql")
                                  ?? throw new InvalidOperationException("Connection string 'PostgreSql' is missing.");
        var connectionString = NormalizePostgresConnectionString(connectionStringRaw);

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

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrWhiteSpace(accessToken) && path.StartsWithSegments("/hubs/metrics"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static string NormalizePostgresConnectionString(string value)
    {
        var raw = value.Trim().Trim('"');
        if (!raw.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !raw.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return raw;
        }

        var uri = new Uri(raw);
        var userInfoParts = uri.UserInfo.Split(':', 2, StringSplitOptions.None);
        var username = userInfoParts.Length > 0 ? Uri.UnescapeDataString(userInfoParts[0]) : string.Empty;
        var password = userInfoParts.Length > 1 ? Uri.UnescapeDataString(userInfoParts[1]) : string.Empty;
        var database = uri.AbsolutePath.Trim('/');

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Username = username,
            Password = password,
            Database = database
        };

        if (!string.IsNullOrWhiteSpace(uri.Query))
        {
            var pairs = uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs)
            {
                var kv = pair.Split('=', 2, StringSplitOptions.None);
                if (kv.Length != 2)
                {
                    continue;
                }

                var key = Uri.UnescapeDataString(kv[0]);
                var val = Uri.UnescapeDataString(kv[1]);
                builder[key] = val;
            }
        }

        return builder.ConnectionString;
    }
}
