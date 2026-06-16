using Domain.Entities;
using Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new ItemConfiguration());
        modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());

        // Seed default admin user (password: Admin@123)
        modelBuilder.Entity<ApplicationUser>().HasData(new ApplicationUser
        {
            Id = 1,
            Username = "admin",
            Email = "admin@productapi.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = "Admin"
        });

        // Seed Dummy Products
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, ProductName = "Wireless Mouse", CreatedBy = "admin", CreatedOn = DateTime.UtcNow },
            new Product { Id = 2, ProductName = "Mechanical Keyboard", CreatedBy = "admin", CreatedOn = DateTime.UtcNow },
            new Product { Id = 3, ProductName = "HD Monitor", CreatedBy = "admin", CreatedOn = DateTime.UtcNow }
        );

        // Seed Dummy Items
        modelBuilder.Entity<Item>().HasData(
            new Item { Id = 1, ProductId = 1, Quantity = 50 },
            new Item { Id = 2, ProductId = 1, Quantity = 150 },
            new Item { Id = 3, ProductId = 2, Quantity = 20 },
            new Item { Id = 4, ProductId = 3, Quantity = 5 }
        );
    }
}
