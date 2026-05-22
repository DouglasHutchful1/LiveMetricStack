using LiveMetricStack.Application.DependencyInjection;
using LiveMetricStack.Application.Alerts;
using LiveMetricStack.Application.Metrics;
using LiveMetricStack.Infrastructure.DependencyInjection;
using LiveMetricStack.Infrastructure.Persistence;
using LiveMetricStack.WebApi.Hubs;
using LiveMetricStack.WebApi.Realtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "LiveMetricStack API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IMetricsRealtimePublisher, SignalRMetricsRealtimePublisher>();
builder.Services.AddSingleton<IAlertsRealtimePublisher, SignalRAlertsRealtimePublisher>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<MetricsHub>("/hubs/metrics");

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<LiveMetricDbContext>();
    dbContext.Database.Migrate();
}

app.Run();
