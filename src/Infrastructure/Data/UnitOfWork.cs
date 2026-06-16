using Application.Interfaces;
using Infrastructure.Data.Repositories;

namespace Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context,
        IProductRepository products,
        IItemRepository items,
        IUserRepository users)
    {
        _context = context;
        Products = products;
        Items = items;
        Users = users;
    }

    public IProductRepository Products { get; }
    public IItemRepository Items { get; }
    public IUserRepository Users { get; }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public void Dispose() => _context.Dispose();
}
