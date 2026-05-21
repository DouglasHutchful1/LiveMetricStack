using LiveMetricStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ApplicationEntity = LiveMetricStack.Domain.Entities.Application;

namespace LiveMetricStack.Infrastructure.Persistence;

public class LiveMetricDbContext(DbContextOptions<LiveMetricDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<ApplicationEntity> Applications => Set<ApplicationEntity>();
    public DbSet<Metric> Metrics => Set<Metric>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<ApiHealthCheck> ApiHealthChecks => Set<ApiHealthCheck>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(150).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.Role).HasMaxLength(50).HasDefaultValue("User").IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<ApplicationEntity>(entity =>
        {
            entity.ToTable("Applications");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Environment).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ApiKey).HasMaxLength(150).IsRequired();
            entity.HasIndex(x => x.ApiKey).IsUnique();
            entity.Property(x => x.Status).HasMaxLength(50).HasDefaultValue("Online").IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<Metric>(entity =>
        {
            entity.ToTable("Metrics");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.MetricName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.MetricValue).HasColumnType("numeric(18,2)");
            entity.Property(x => x.Unit).HasMaxLength(50);
            entity.Property(x => x.RecordedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(x => x.Application)
                .WithMany(x => x.Metrics)
                .HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(255).IsRequired();
            entity.Property(x => x.Severity).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(x => x.Application)
                .WithMany(x => x.Events)
                .HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Alert>(entity =>
        {
            entity.ToTable("Alerts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(255).IsRequired();
            entity.Property(x => x.Severity).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(x => x.Application)
                .WithMany(x => x.Alerts)
                .HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApiHealthCheck>(entity =>
        {
            entity.ToTable("ApiHealthChecks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Endpoint).HasMaxLength(255).IsRequired();
            entity.Property(x => x.CheckedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(x => x.Application)
                .WithMany(x => x.ApiHealthChecks)
                .HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
