using Application.DTOs.Auth;
using Application.DTOs.Item;
using Application.DTOs.Product;

namespace Application.Interfaces;

public interface IProductService
{
    Task<PagedResponse<ProductResponse>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<ProductResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ProductWithItemsResponse> GetWithItemsAsync(int id, CancellationToken ct = default);
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface IItemService
{
    Task<IEnumerable<ItemResponse>> GetByProductIdAsync(int productId, CancellationToken ct = default);
    Task<ItemResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ItemResponse> CreateAsync(CreateItemRequest request, CancellationToken ct = default);
    Task<ItemResponse> UpdateAsync(int id, UpdateItemRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface IAuthService
{
    Task<TokenResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
    Task RevokeTokenAsync(RevokeTokenRequest request, CancellationToken ct = default);
}

public interface ICurrentUserService
{
    string Username { get; }
    int UserId { get; }
    string Role { get; }
    bool IsAuthenticated { get; }
}

public interface IJwtTokenService
{
    string GenerateAccessToken(Domain.Entities.ApplicationUser user);
    Domain.Entities.RefreshToken GenerateRefreshToken(int userId);
    System.Security.Claims.ClaimsPrincipal? ValidateExpiredToken(string token);
}
