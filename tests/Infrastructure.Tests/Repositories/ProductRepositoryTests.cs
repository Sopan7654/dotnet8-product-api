using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories;

public class ProductRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("ProductRepoTest_" + Guid.NewGuid())
            .Options;
        _context = new ApplicationDbContext(options);
        _repository = new ProductRepository(_context);
    }

    private Product CreateProduct(string name = "Test Product") => new()
    {
        ProductName = name,
        CreatedBy = "test",
        CreatedOn = DateTime.UtcNow
    };

    [Fact]
    public async Task AddAsync_ThenSave_PersistsProduct()
    {
        var product = CreateProduct();
        await _repository.AddAsync(product);
        await _context.SaveChangesAsync();

        var saved = await _context.Products.FindAsync(product.Id);
        Assert.NotNull(saved);
        Assert.Equal("Test Product", saved.ProductName);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _repository.GetByIdAsync(9999);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedResults()
    {
        for (int i = 1; i <= 15; i++)
        {
            await _repository.AddAsync(CreateProduct($"Product {i}"));
        }
        await _context.SaveChangesAsync();

        var (items, total) = await _repository.GetAllAsync(1, 10);

        Assert.Equal(15, total);
        Assert.Equal(10, items.Count());
    }

    [Fact]
    public async Task DeleteAsync_RemovesProduct()
    {
        var product = CreateProduct("To Delete");
        await _repository.AddAsync(product);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(product);
        await _context.SaveChangesAsync();

        var found = await _context.Products.FindAsync(product.Id);
        Assert.Null(found);
    }

    public void Dispose() => _context.Dispose();
}
