using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationUser?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => await _context.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

    public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<ApplicationUser?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default)
        => await _context.Users.AnyAsync(u => u.Username == username, ct);

    public async Task<ApplicationUser> AddAsync(ApplicationUser user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
        return user;
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken ct = default)
        => await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == token, ct);

    public async Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct = default)
        => await _context.RefreshTokens.AddAsync(token, ct);

    public Task RevokeRefreshTokenAsync(RefreshToken token, CancellationToken ct = default)
    {
        token.IsRevoked = true;
        _context.RefreshTokens.Update(token);
        return Task.CompletedTask;
    }
}
