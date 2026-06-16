using Application.DTOs.Item;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ItemService : IItemService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<ItemService> _logger;

    public ItemService(IUnitOfWork uow, IMapper mapper, ILogger<ItemService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<ItemResponse>> GetByProductIdAsync(int productId, CancellationToken ct = default)
    {
        // Validate product exists
        var product = await _uow.Products.GetByIdAsync(productId, ct)
            ?? throw new NotFoundException(nameof(Product), productId);

        var items = await _uow.Items.GetByProductIdAsync(productId, ct);
        return _mapper.Map<IEnumerable<ItemResponse>>(items);
    }

    public async Task<ItemResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var item = await _uow.Items.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Item), id);
        return _mapper.Map<ItemResponse>(item);
    }

    public async Task<ItemResponse> CreateAsync(CreateItemRequest request, CancellationToken ct = default)
    {
        // Validate product exists
        _ = await _uow.Products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        _logger.LogInformation("Creating item for product {ProductId}", request.ProductId);

        var item = new Item { ProductId = request.ProductId, Quantity = request.Quantity };
        await _uow.Items.AddAsync(item, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<ItemResponse>(item);
    }

    public async Task<ItemResponse> UpdateAsync(int id, UpdateItemRequest request, CancellationToken ct = default)
    {
        var item = await _uow.Items.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Item), id);

        item.Quantity = request.Quantity;
        await _uow.Items.UpdateAsync(item, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<ItemResponse>(item);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var item = await _uow.Items.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Item), id);

        await _uow.Items.DeleteAsync(item, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
