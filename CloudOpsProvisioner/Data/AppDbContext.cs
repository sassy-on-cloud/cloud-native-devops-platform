using CloudOpsProvisioner.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudOpsProvisioner.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<ProvisioningRequest> ProvisioningRequests =>
        Set<ProvisioningRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProvisioningRequest>(entity =>
        {
            entity.HasKey(request => request.Id);

            entity.Property(request => request.Environment)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(request => request.AwsRegion)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(request => request.DeploymentPlatform)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(request => request.Status)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(request => request.RequestedAt)
                .IsRequired();

            entity.HasIndex(request => request.RequestedAt);
        });
    }
}
