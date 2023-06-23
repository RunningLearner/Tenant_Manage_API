using Challenge04_TenantManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace Challenge04_TenantManagementApi.Data;

public sealed class GraphDbContext : DbContext
{
    public GraphDbContext(DbContextOptions<GraphDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Group> Groups { get; set; }
}