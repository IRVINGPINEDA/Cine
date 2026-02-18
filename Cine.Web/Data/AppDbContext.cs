using Cine.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cine.Web.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Client> Clients => Set<Client>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Movie>(entity =>
        {
            entity.Property(x => x.Genre).HasConversion<string>();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
        });

        builder.Entity<Client>(entity =>
        {
            entity.Property(x => x.RegisteredAt).HasDefaultValueSql("NOW()");
            entity.HasIndex(x => x.Email);
        });
    }
}
