using DynamicFilteringApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DynamicFilteringApi.Database;

public class ProductDbContext : DbContext
{ 
    public DbSet<Product> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Keyboard", Category = "Electronics", Price = 20.00m, IsActive = true },
            new Product { Id = 2, Name = "Mouse", Category = "Electronics", Price = 10.00m, IsActive = true },
            new Product { Id = 3, Name = "Monitor", Category = "Electronics", Price = 100.00m, IsActive = false },
            new Product { Id = 4, Name = "Headphones", Category = "Electronics", Price = 15.00m, IsActive = true },
            new Product { Id = 5, Name = "Speakers", Category = "Electronics", Price = 25.00m, IsActive = false },
            new Product { Id = 6, Name = "Oats", Category = "Food", Price = 500.00m, IsActive = true }
        );

        base.OnModelCreating(modelBuilder);
    }
}
