using Application.DTOs.Product;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<ILogger<ProductService>> _loggerMock = new();
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _currentUserMock.Setup(u => u.Username).Returns("testadmin");
        _service = new ProductService(_uowMock.Object, _mapperMock.Object, _currentUserMock.Object, _loggerMock.Object);
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsProduct_WhenExists()
    {
        // Arrange
        var product = new Product { Id = 1, ProductName = "Widget", CreatedBy = "admin", CreatedOn = DateTime.UtcNow };
        var expected = new ProductResponse(1, "Widget", "admin", product.CreatedOn, null, null);

        _uowMock.Setup(u => u.Products.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _mapperMock.Setup(m => m.Map<ProductResponse>(product)).Returns(expected);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundException_WhenNotFound()
    {
        _uowMock.Setup(u => u.Products.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(999));
    }

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_CreatesProduct_AndReturnsDto()
    {
        // Arrange
        var request = new CreateProductRequest("Test Product");
        var product = new Product { Id = 5, ProductName = "Test Product", CreatedBy = "testadmin", CreatedOn = DateTime.UtcNow };
        var expected = new ProductResponse(5, "Test Product", "testadmin", product.CreatedOn, null, null);

        _uowMock.Setup(u => u.Products.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken _) => { p.Id = 5; return p; });
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<ProductResponse>(It.IsAny<Product>())).Returns(expected);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.Equal("Test Product", result.ProductName);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundException_WhenProductNotFound()
    {
        _uowMock.Setup(u => u.Products.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UpdateAsync(99, new UpdateProductRequest("New Name")));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesModifiedFields()
    {
        // Arrange
        var product = new Product { Id = 1, ProductName = "Old", CreatedBy = "admin", CreatedOn = DateTime.UtcNow };
        var updated = product with { ProductName = "New" };
        var expected = new ProductResponse(1, "New", "admin", product.CreatedOn, "testadmin", DateTime.UtcNow);

        _uowMock.Setup(u => u.Products.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _uowMock.Setup(u => u.Products.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<ProductResponse>(It.IsAny<Product>())).Returns(expected);

        // Act
        var result = await _service.UpdateAsync(1, new UpdateProductRequest("New"));

        // Assert
        Assert.Equal("testadmin", result.ModifiedBy);
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_CallsDeleteAndSave_WhenProductExists()
    {
        var product = new Product { Id = 1, ProductName = "ToDelete", CreatedBy = "admin", CreatedOn = DateTime.UtcNow };
        _uowMock.Setup(u => u.Products.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _uowMock.Setup(u => u.Products.DeleteAsync(product, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _service.DeleteAsync(1);

        _uowMock.Verify(u => u.Products.DeleteAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedResponse()
    {
        var products = new List<Product>
        {
            new() { Id = 1, ProductName = "A", CreatedBy = "admin", CreatedOn = DateTime.UtcNow },
            new() { Id = 2, ProductName = "B", CreatedBy = "admin", CreatedOn = DateTime.UtcNow }
        };
        var dtos = products.Select(p => new ProductResponse(p.Id, p.ProductName, p.CreatedBy, p.CreatedOn, null, null));

        _uowMock.Setup(u => u.Products.GetAllAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, 2));
        _mapperMock.Setup(m => m.Map<IEnumerable<ProductResponse>>(products)).Returns(dtos);

        var result = await _service.GetAllAsync(1, 10);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
    }
}
