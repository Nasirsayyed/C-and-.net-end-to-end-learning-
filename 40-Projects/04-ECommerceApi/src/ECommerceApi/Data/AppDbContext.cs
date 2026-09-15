using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Product>(entity => entity.HasKey(p => p.Id));

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);

            // Order.Lines is a read-only property wrapping the private `_lines` field —
            // this tells EF Core to read/write through that field directly (see module
            // 27-DDD: the aggregate's public surface stays encapsulated even to the ORM).
            entity.Metadata.FindNavigation(nameof(Order.Lines))!.SetPropertyAccessMode(PropertyAccessMode.Field);

            entity.HasMany(o => o.Lines)
                .WithOne()
                .HasForeignKey("OrderId") // shadow FK — OrderLine doesn't need to expose it
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderLine>(entity => entity.HasKey(l => l.Id));
    }
}
