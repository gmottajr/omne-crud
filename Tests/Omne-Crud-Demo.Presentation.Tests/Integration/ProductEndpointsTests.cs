using System.Net;
using System.Net.Http.Json;
using Omne_Crud_Demo.Core.Models;
using Omne_Crud_Demo.Core.Models.Requests;

namespace Omne_Crud_Demo.Presentation.Tests.Integration;

[Collection(PresentationIntegrationCollection.Name)]
public sealed class ProductEndpointsTests : IAsyncLifetime
{
    private readonly PresentationWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProductEndpointsTests(
        PresentationWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync()
    {
        return _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task PostProducts_ShouldCreateProduct()
    {
        var request = new CreateProductRequest
        {
            Name = "Mechanical Keyboard",
            Price = 249.90m,
            Description = "RGB mechanical keyboard",
            Sku = "SKU-001"
        };

        var response =
            await _client.PostAsJsonAsync(
                "/products",
                request);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnProduct_WhenProductExists()
    {
        var id = await CreateProductAsync(
            "SKU-001",
            "Mechanical Keyboard");

        var response =
            await _client.GetAsync(
                $"/products/{id}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var product =
            await response.Content
                .ReadFromJsonAsync<ProductDto>();

        Assert.NotNull(product);
        Assert.Equal(id, product.Id);
        Assert.Equal("SKU-001", product.Sku);
        Assert.Equal(
            "Mechanical Keyboard",
            product.Name);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var response =
            await _client.GetAsync(
                "/products/999999");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_ShouldReturnAllProducts()
    {
        await CreateProductAsync(
            "SKU-001",
            "Keyboard");

        await CreateProductAsync(
            "SKU-002",
            "Mouse");

        var response =
            await _client.GetAsync("/products");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var products =
            await response.Content
                .ReadFromJsonAsync<List<ProductDto>>();

        Assert.NotNull(products);
        Assert.Equal(2, products.Count);
    }

    [Fact]
    public async Task PutProduct_ShouldUpdateProduct()
    {
        var id = await CreateProductAsync(
            "SKU-001",
            "Keyboard");

        var request = new UpdateProductRequest
        {
            Name = "Gaming Keyboard",
            Price = 399.90m,
            Description =
                "Updated mechanical gaming keyboard"
        };

        var response =
            await _client.PutAsJsonAsync(
                $"/products/{id}",
                request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var getResponse =
            await _client.GetAsync(
                $"/products/{id}");

        var product =
            await getResponse.Content
                .ReadFromJsonAsync<ProductDto>();

        Assert.NotNull(product);
        Assert.Equal(
            "Gaming Keyboard",
            product.Name);
        Assert.Equal(
            399.90m,
            product.Price);
    }

    [Fact]
    public async Task DeleteProduct_ShouldDeleteProduct()
    {
        var id = await CreateProductAsync(
            "SKU-001",
            "Keyboard");

        var response =
            await _client.DeleteAsync(
                $"/products/{id}");

        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);

        var getResponse =
            await _client.GetAsync(
                $"/products/{id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            getResponse.StatusCode);
    }

    private async Task<int> CreateProductAsync(
        string sku,
        string name)
    {
        var request = new CreateProductRequest
        {
            Name = name,
            Price = 149.90m,
            Description =
                $"Description for {name}",
            Sku = sku
        };

        var response =
            await _client.PostAsJsonAsync(
                "/products",
                request);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<int>();
    }
}
