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
        // DB가 없으면 만듭니다.
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // DbContextOptionsBuilder를 통해 로그 레벨을 설정합니다.
        optionsBuilder
            .UseLoggerFactory(LoggerFactory.Create(builder
                => builder.AddFilter((category, level) => level == LogLevel.Information)));

        optionsBuilder
            .AddInterceptors(new SoftDeleteInterceptor());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasQueryFilter(user => user.IsDeleted == false);

        modelBuilder.Entity<User>()
            .Property(user => user.CreatedDateTime)
            .HasConversion<long>();

        modelBuilder.Entity<Group>()
            .Property(group => group.CreatedDateTime)
            .HasConversion<long>();

        modelBuilder.Entity<User>()
            .HasQueryFilter(group => group.IsDeleted == false);
    }
}