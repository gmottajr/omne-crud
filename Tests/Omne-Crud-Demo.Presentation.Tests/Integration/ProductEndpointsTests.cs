using System.Net;
using System.Net.Http.Json;
using Omne_Crud_Demo.Core.Common.Services;
using Omne_Crud_Demo.Core.Models;
using Omne_Crud_Demo.Core.Models.Requests;

namespace Omne_Crud_Demo.Presentation.Tests.Integration;

[Collection(PresentationIntegrationCollection.Name)]
public sealed class ProductEndpointsTests : IAsyncLifetime
{
    private const string UPDATE_PROD = "Gaming Keyboard";
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

        var response = await _client.GetAsync($"/products/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var product = await response.Content.ReadFromJsonAsync<ApplicationResponse<ProductDto>>();

        Assert.NotNull(product.Data);
        Assert.Equal(id, product.Data.Id);
        Assert.Equal("SKU-001", product.Data.Sku);
        Assert.Equal(
            "Mechanical Keyboard",
            product.Data.Name);
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

        var response = await _client.GetAsync("/products");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var products =
            await response.Content
                .ReadFromJsonAsync<ApplicationResponse<List<ProductDto>>>();

        Assert.NotNull(products.Data);
        Assert.Equal(2, products.Data.Count);
    }

    [Fact]
    public async Task FilterProducts_ShouldReturnOnlyProductsMatchingAllFilters()
    {
        await CreateProductAsync(
            "KEY-001",
            "Mechanical Keyboard",
            249.90m);

        await CreateProductAsync(
            "KEY-002",
            "Office Keyboard",
            89.90m);

        await CreateProductAsync(
            "MOUSE-001",
            "Gaming Mouse",
            149.90m);

        var response = await _client.GetAsync(
            "/products/filter?name=keyboard&sku=001&minPrice=200&maxPrice=300" +
            "&createdFrom=2000-01-01&createdTo=2100-01-01");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content
            .ReadFromJsonAsync<ApplicationResponse<List<ProductDto>>>();

        Assert.NotNull(products);
        Assert.NotNull(products.Data);
        var product = Assert.Single(products.Data);
        Assert.Equal("Mechanical Keyboard", product.Name);
    }

    [Fact]
    public async Task FilterProducts_ShouldReturnBadRequest_WhenRangeIsInvalid()
    {
        var response = await _client.GetAsync(
            "/products/filter?minPrice=100&maxPrice=50");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutProduct_ShouldUpdateProduct()
    {
        var id = await CreateProductAsync(
            "SKU-33378",
            "Webcam");

        var request = new UpdateProductRequest
        {
            Name = UPDATE_PROD,
            Price = 399.90m,
            Description =
                "Updated mechanical gaming keyboard"
        };

        var response =
            await _client.PutAsJsonAsync(
                $"/products/{id}",
                request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var getResponse = await _client.GetAsync($"/products/{id}");

        var product = await getResponse.Content.ReadFromJsonAsync<ApplicationResponse<ProductDto>>();

        Assert.NotNull(product.Data);
        Assert.Equal(UPDATE_PROD, product.Data.Name);
        Assert.Equal(
            399.90m,
            product.Data.Price);
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

    private Task<int> CreateProductAsync(string sku, string name)
    {
        return CreateProductAsync(sku, name, GeneratePriceRamdon());
    }

    private async Task<int> CreateProductAsync(
        string sku,
        string name,
        decimal price)
    {
        var request = new CreateProductRequest
        {
            Name = name,
            Price = price,
            Description = $"This is my Description for {name}",
            Sku = sku
        };

        var response =
            await _client.PostAsJsonAsync(
                "/products",
                request);

        var body = await response.Content.ReadFromJsonAsync<ApplicationResponse<int>>();

        Assert.True(
            response.IsSuccessStatusCode,
            $"POST /products failed with {(int)response.StatusCode} " +
            $"{response.StatusCode}. Body: {body}");

        return body.Data;
    }

    private static decimal GeneratePriceRamdon()
    {
        var rnd = new Random();
        decimal minValue = 10.5m;
        decimal maxValue = 50.25m;

        decimal randomRangeDecimal = minValue + (decimal)rnd.NextDouble() * (maxValue - minValue);
        return Math.Round(randomRangeDecimal, 2); 

    }
}
