using Application.DTOs.Product;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IUnitOfWork uow, IMapper mapper, ICurrentUserService currentUser, ILogger<ProductService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<PagedResponse<ProductResponse>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching products - Page {Page}, Size {Size}", pageNumber, pageSize);
        var (items, total) = await _uow.Products.GetAllAsync(pageNumber, pageSize, ct);
        var dtos = _mapper.Map<IEnumerable<ProductResponse>>(items);
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);
        return new PagedResponse<ProductResponse>(dtos, total, pageNumber, pageSize, totalPages);
    }

    public async Task<ProductResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var product = await _uow.Products.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Product), id);
        return _mapper.Map<ProductResponse>(product);
    }

    public async Task<ProductWithItemsResponse> GetWithItemsAsync(int id, CancellationToken ct = default)
    {
        var product = await _uow.Products.GetByIdWithItemsAsync(id, ct)
            ?? throw new NotFoundException(nameof(Product), id);
        return _mapper.Map<ProductWithItemsResponse>(product);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating product: {Name} by {User}", request.ProductName, _currentUser.Username);

        var product = new Product
        {
            ProductName = request.ProductName,
            CreatedBy = _currentUser.Username,
            CreatedOn = DateTime.UtcNow
        };

        await _uow.Products.AddAsync(product, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Product created with ID: {Id}", product.Id);
        return _mapper.Map<ProductResponse>(product);
    }

    public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request, CancellationToken ct = default)
    {
        var product = await _uow.Products.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Product), id);

        _logger.LogInformation("Updating product {Id} by {User}", id, _currentUser.Username);
        product.ProductName = request.ProductName;
        product.ModifiedBy = _currentUser.Username;
        product.ModifiedOn = DateTime.UtcNow;

        await _uow.Products.UpdateAsync(product, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<ProductResponse>(product);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var product = await _uow.Products.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Product), id);

        _logger.LogInformation("Deleting product {Id} by {User}", id, _currentUser.Username);
        await _uow.Products.DeleteAsync(product, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
