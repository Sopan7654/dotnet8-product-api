using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly ApplicationDbContext _context;

    public ItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Item>> GetByProductIdAsync(int productId, CancellationToken ct = default)
    {
        return await _context.Items
            .AsNoTracking()
            .Where(i => i.ProductId == productId)
            .ToListAsync(ct);
    }

    public async Task<Item?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<Item> AddAsync(Item item, CancellationToken ct = default)
    {
        await _context.Items.AddAsync(item, ct);
        return item;
    }

    public Task UpdateAsync(Item item, CancellationToken ct = default)
    {
        _context.Items.Update(item);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Item item, CancellationToken ct = default)
    {
        _context.Items.Remove(item);
        return Task.CompletedTask;
    }
}
