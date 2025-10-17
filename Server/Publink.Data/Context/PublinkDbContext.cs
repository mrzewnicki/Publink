using Microsoft.EntityFrameworkCore;

namespace Publink.Data.Context;

public class PublinkDbContext : DbContext
{
    public PublinkDbContext(DbContextOptions<PublinkDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Configure your entities here when they are added
    }
}
