using Microsoft.EntityFrameworkCore;
using SinformWcApi.Entities;

namespace SinformWcApi;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.ApiKey).IsRequired().HasMaxLength(100);

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.ApiKey).IsUnique();
        });
    }
}
