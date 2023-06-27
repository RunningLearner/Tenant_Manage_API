using Challenge04_TenantManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace Challenge04_TenantManagementApi.Data;

public sealed class GraphDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Group> Groups { get; set; }

    public GraphDbContext(DbContextOptions<GraphDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .Property(e => e.CreatedDateTime)
            .HasConversion<long>();

        modelBuilder.Entity<Group>()
            .Property(e => e.CreatedDateTime)
            .HasConversion<long>();
    }
}