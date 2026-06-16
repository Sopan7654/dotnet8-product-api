using System.Net;
using System.Net.Http.Json;
using Application.DTOs.Auth;
using Application.DTOs.Product;

namespace API.Tests;

/// <summary>
/// Integration tests using an in-memory database via WebApplicationFactory.
/// Tests the full HTTP pipeline for auth and product endpoints.
/// </summary>
public class ProductsIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public ProductsIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest("admin", "Admin@123"));
        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return token!.AccessToken;
    }

    // ── Auth Tests ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_Returns200_WithValidCredentials()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest("admin", "Admin@123"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(body?.AccessToken);
    }

    [Fact]
    public async Task Login_Returns401_WithInvalidCredentials()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest("admin", "WrongPassword"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── Products Tests ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProducts_Returns401_WhenUnauthenticated()
    {
        var response = await _client.GetAsync("/api/v1/products");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_Returns200_WhenAuthenticated()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/products");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_Returns201_WithValidRequest()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/v1/products",
            new CreateProductRequest("Integration Test Product"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.Equal("Integration Test Product", product?.ProductName);
    }

    [Fact]
    public async Task CreateProduct_Returns422_WithEmptyName()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/v1/products",
            new CreateProductRequest(""));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task GetProduct_Returns404_WhenNotFound()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/products/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_Returns204_WhenExists()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create first
        var create = await _client.PostAsJsonAsync("/api/v1/products",
            new CreateProductRequest("Delete Me"));
        var product = await create.Content.ReadFromJsonAsync<ProductResponse>();

        // Delete
        var response = await _client.DeleteAsync($"/api/v1/products/{product!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
