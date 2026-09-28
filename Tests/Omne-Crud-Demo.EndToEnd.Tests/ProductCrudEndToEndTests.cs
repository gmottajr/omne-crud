using System.Net;
using System.Net.Http.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using Omne_Crud_Demo.Core.Common.Services;
using Omne_Crud_Demo.Core.Models;
using Omne_Crud_Demo.Core.Models.Requests;

namespace Omne_Crud_Demo.EndToEnd.Tests;

[Collection(EndToEndCollection.Name)]
public sealed class ProductCrudEndToEndTests : PageTest
{
    private readonly AppHostEndToEndFixture _fixture;

    public ProductCrudEndToEndTests(AppHostEndToEndFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Server_ShouldExposeFastEndpointsOpenApiAndScalarReference()
    {
        using var client = _fixture.CreateServerClient();

        using var openApiResponse = await client.GetAsync("/openapi/v1.json");
        Assert.Equal(HttpStatusCode.OK, openApiResponse.StatusCode);

        var openApiDocument = await openApiResponse.Content.ReadAsStringAsync();
        Assert.Contains("/products", openApiDocument, StringComparison.Ordinal);

        using var scalarResponse = await client.GetAsync("/scalar/v1");
        Assert.Equal(HttpStatusCode.OK, scalarResponse.StatusCode);

        using var weatherResponse = await client.GetAsync("/api/weatherforecast");
        Assert.Equal(HttpStatusCode.NotFound, weatherResponse.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_ShouldPersistAndRemainVisibleAfterReload()
    {
        var sku = NewSku("CREATE");
        const string name = "E2E created product";

        try
        {
            await NavigateToFrontendAsync();
            await Page.GetByRole(
                    AriaRole.Button,
                    new PageGetByRoleOptions { Name = "Add new product" })
                .ClickAsync();
            await Expect(Page.GetByRole(
                    AriaRole.Dialog,
                    new PageGetByRoleOptions { Name = "Add new product" }))
                .ToBeVisibleAsync();
            await FillProductFormAsync(sku, name, "145.90", "Product created through the real frontend.");
            await Page.GetByRole(
                    AriaRole.Button,
                    new PageGetByRoleOptions { Name = "Create product" })
                .ClickAsync();

            await Expect(Page.GetByText("Product created successfully."))
                .ToBeVisibleAsync();
            await Expect(ProductCard(sku)).ToContainTextAsync(name);

            await Page.ReloadAsync(new PageReloadOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded
            });

            await Expect(ProductCard(sku)).ToContainTextAsync(name);

            var persistedProduct = await GetProductBySkuAsync(sku);
            Assert.NotNull(persistedProduct);
            Assert.Equal(name, persistedProduct.Name);
        }
        finally
        {
            await DeleteProductBySkuIfExistsAsync(sku);
        }
    }

    [Fact]
    public async Task ListProducts_ShouldDisplayProductPersistedThroughApi()
    {
        var sku = NewSku("LIST");
        const string name = "E2E listed product";
        var productId = await CreateProductThroughApiAsync(
            sku,
            name,
            219.75m,
            "Product arranged through the real API for the list scenario.");

        try
        {
            await NavigateToFrontendAsync();

            var productCard = ProductCard(sku);
            await Expect(productCard).ToBeVisibleAsync();
            await Expect(productCard).ToContainTextAsync(name);
        }
        finally
        {
            await DeleteProductIfExistsAsync(productId);
        }
    }

    [Fact]
    public async Task FilterProducts_ShouldApplyAndClearFiltersFromFrontend()
    {
        var marker = Guid.NewGuid().ToString("N")[..8];
        var matchingName = $"E2E filter {marker} keyboard";
        var matchingSkuFragment = $"E2E-FLT-{marker}";
        var productIds = new List<int>();

        try
        {
            var expectedSku = $"{matchingSkuFragment}-TARGET";
            productIds.Add(await CreateProductThroughApiAsync(
                expectedSku,
                matchingName,
                225m,
                "Product expected from the browser filter scenario."));

            var nameMismatchSku = $"{matchingSkuFragment}-NAME";
            productIds.Add(await CreateProductThroughApiAsync(
                nameMismatchSku,
                $"E2E unrelated {marker} mouse",
                225m,
                "Product excluded by the partial name filter."));

            var skuMismatch = $"E2E-OTHER-{marker}";
            productIds.Add(await CreateProductThroughApiAsync(
                skuMismatch,
                matchingName,
                225m,
                "Product excluded by the partial SKU filter."));

            var priceMismatchSku = $"{matchingSkuFragment}-PRICE";
            productIds.Add(await CreateProductThroughApiAsync(
                priceMismatchSku,
                matchingName,
                400m,
                "Product excluded by the price range filter."));

            await NavigateToFrontendAsync();
            await Expect(ProductCard(expectedSku)).ToBeVisibleAsync();

            await Page.GetByRole(
                    AriaRole.Textbox,
                    new PageGetByRoleOptions { Name = "Product name" })
                .FillAsync($"filter {marker} keyboard");
            await Page.GetByRole(
                    AriaRole.Textbox,
                    new PageGetByRoleOptions { Name = "Product SKU" })
                .FillAsync(matchingSkuFragment);
            await Page.GetByRole(
                    AriaRole.Spinbutton,
                    new PageGetByRoleOptions { Name = "Minimum price" })
                .FillAsync("200");
            await Page.GetByRole(
                    AriaRole.Spinbutton,
                    new PageGetByRoleOptions { Name = "Maximum price" })
                .FillAsync("250");
            await Page.GetByLabel("Created from").FillAsync("2000-01-01");
            await Page.GetByLabel("Created to").FillAsync("2099-12-31");
            await Page.GetByRole(
                    AriaRole.Button,
                    new PageGetByRoleOptions { Name = "Apply filters" })
                .ClickAsync();

            await Expect(ProductCard(expectedSku)).ToBeVisibleAsync();
            await Expect(ProductCard(nameMismatchSku)).ToHaveCountAsync(0);
            await Expect(ProductCard(skuMismatch)).ToHaveCountAsync(0);
            await Expect(ProductCard(priceMismatchSku)).ToHaveCountAsync(0);

            await Page.GetByLabel("Created from").FillAsync("2100-01-01");
            await Page.GetByLabel("Created to").FillAsync("2100-12-31");
            await Page.GetByRole(
                    AriaRole.Button,
                    new PageGetByRoleOptions { Name = "Apply filters" })
                .ClickAsync();
            await Expect(Page.GetByRole(
                    AriaRole.Heading,
                    new PageGetByRoleOptions { Name = "No products found" }))
                .ToBeVisibleAsync();

            await Page.GetByRole(
                    AriaRole.Button,
                    new PageGetByRoleOptions { Name = "Clear filters" })
                .ClickAsync();
            await Expect(ProductCard(expectedSku)).ToBeVisibleAsync();
            await Expect(ProductCard(nameMismatchSku)).ToBeVisibleAsync();
        }
        finally
        {
            foreach (var productId in productIds)
            {
                await DeleteProductIfExistsAsync(productId);
            }
        }
    }

    [Fact]
    public async Task UpdateProduct_ShouldPersistChangesAndRemainUpdatedAfterReload()
    {
        var sku = NewSku("UPDATE");
        const string originalName = "E2E product before update";
        const string updatedName = "E2E product after update";
        const decimal updatedPrice = 321.45m;
        const string updatedDescription = "Product updated through the real frontend.";
        var productId = await CreateProductThroughApiAsync(
            sku,
            originalName,
            120.50m,
            "Product arranged through the real API for the update scenario.");

        try
        {
            await NavigateToFrontendAsync();

            var productCard = ProductCard(sku);
            await Expect(productCard).ToContainTextAsync(originalName);
            await productCard.GetByRole(
                    AriaRole.Button,
                    new LocatorGetByRoleOptions { Name = "Edit" })
                .ClickAsync();

            await Page.GetByRole(
                    AriaRole.Textbox,
                    new PageGetByRoleOptions { Name = "Name", Exact = true })
                .FillAsync(updatedName);
            await Page.GetByRole(
                    AriaRole.Spinbutton,
                    new PageGetByRoleOptions { Name = "Price", Exact = true })
                .FillAsync(updatedPrice.ToString(System.Globalization.CultureInfo.InvariantCulture));
            await Page.GetByRole(
                    AriaRole.Textbox,
                    new PageGetByRoleOptions { Name = "Description" })
                .FillAsync(updatedDescription);
            await Page.GetByRole(
                    AriaRole.Button,
                    new PageGetByRoleOptions { Name = "Save changes" })
                .ClickAsync();

            await Expect(Page.GetByText("Product updated successfully."))
                .ToBeVisibleAsync();
            await Expect(ProductCard(sku)).ToContainTextAsync(updatedName);

            await Page.ReloadAsync(new PageReloadOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded
            });

            await Expect(ProductCard(sku)).ToContainTextAsync(updatedName);

            var persistedProduct = await GetProductByIdAsync(productId);
            Assert.Equal(updatedName, persistedProduct.Name);
            Assert.Equal(updatedPrice, persistedProduct.Price);
            Assert.Equal(updatedDescription, persistedProduct.Description);
        }
        finally
        {
            await DeleteProductIfExistsAsync(productId);
        }
    }

    [Fact]
    public async Task DeleteProduct_ShouldRemovePersistedProductAndRemainAbsentAfterReload()
    {
        var sku = NewSku("DELETE");
        const string name = "E2E product to delete";
        var productId = await CreateProductThroughApiAsync(
            sku,
            name,
            49.99m,
            "Product arranged through the real API for the delete scenario.");

        try
        {
            await NavigateToFrontendAsync();

            var productCard = ProductCard(sku);
            await Expect(productCard).ToContainTextAsync(name);
            Page.Dialog += AcceptDialogAsync;

            await productCard.GetByRole(
                    AriaRole.Button,
                    new LocatorGetByRoleOptions { Name = $"Delete {name}" })
                .ClickAsync();

            await Expect(Page.GetByText("Product deleted successfully."))
                .ToBeVisibleAsync();
            await Expect(ProductCard(sku)).ToHaveCountAsync(0);

            await Page.ReloadAsync(new PageReloadOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded
            });

            await Expect(ProductCard(sku)).ToHaveCountAsync(0);

            using var client = _fixture.CreateServerClient();
            using var response = await client.GetAsync($"/products/{productId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        finally
        {
            await DeleteProductIfExistsAsync(productId);
        }
    }

    private async Task NavigateToFrontendAsync()
    {
        await Page.GotoAsync(
            _fixture.WebFrontendEndpoint.AbsoluteUri,
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        await Expect(Page.GetByRole(
                AriaRole.Heading,
                new PageGetByRoleOptions { Name = "Registered products" }))
            .ToBeVisibleAsync();
    }

    private async Task FillProductFormAsync(
        string sku,
        string name,
        string price,
        string description)
    {
        await Page.GetByRole(
                AriaRole.Textbox,
                new PageGetByRoleOptions { Name = "SKU", Exact = true })
            .FillAsync(sku);
        await Page.GetByRole(
                AriaRole.Textbox,
                new PageGetByRoleOptions { Name = "Name", Exact = true })
            .FillAsync(name);
        await Page.GetByRole(
                AriaRole.Spinbutton,
                new PageGetByRoleOptions { Name = "Price", Exact = true })
            .FillAsync(price);
        await Page.GetByRole(
                AriaRole.Textbox,
                new PageGetByRoleOptions { Name = "Description" })
            .FillAsync(description);
    }

    private ILocator ProductCard(string sku)
    {
        return Page.Locator("article.product-card")
            .Filter(new LocatorFilterOptions { HasTextString = $"SKU: {sku}" });
    }

    private async Task<int> CreateProductThroughApiAsync(
        string sku,
        string name,
        decimal price,
        string description)
    {
        using var client = _fixture.CreateServerClient();
        using var response = await client.PostAsJsonAsync(
            "/products",
            new CreateProductRequest
            {
                Sku = sku,
                Name = name,
                Price = price,
                Description = description
            });

        response.EnsureSuccessStatusCode();

        var body = await response.Content
            .ReadFromJsonAsync<ApplicationResponse<int>>();

        Assert.NotNull(body);
        Assert.True(body.Success, body.ErrorMessage);
        Assert.True(body.Data > 0);

        return body.Data;
    }

    private async Task<ProductDto> GetProductByIdAsync(int productId)
    {
        using var client = _fixture.CreateServerClient();
        using var response = await client.GetAsync($"/products/{productId}");
        response.EnsureSuccessStatusCode();

        var body = await response.Content
            .ReadFromJsonAsync<ApplicationResponse<ProductDto>>();

        Assert.NotNull(body);
        Assert.True(body.Success, body.ErrorMessage);
        Assert.NotNull(body.Data);

        return body.Data;
    }

    private async Task<ProductDto?> GetProductBySkuAsync(string sku)
    {
        using var client = _fixture.CreateServerClient();
        using var response = await client.GetAsync(
            $"/products/sku/{Uri.EscapeDataString(sku)}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var body = await response.Content
            .ReadFromJsonAsync<ApplicationResponse<ProductDto>>();

        Assert.NotNull(body);
        Assert.True(body.Success, body.ErrorMessage);
        Assert.NotNull(body.Data);

        return body.Data;
    }

    private async Task DeleteProductBySkuIfExistsAsync(string sku)
    {
        var product = await GetProductBySkuAsync(sku);

        if (product is not null)
        {
            await DeleteProductIfExistsAsync(product.Id);
        }
    }

    private async Task DeleteProductIfExistsAsync(int productId)
    {
        using var client = _fixture.CreateServerClient();
        using var response = await client.DeleteAsync($"/products/{productId}");

        Assert.True(
            response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound,
            $"Cleanup DELETE /products/{productId} returned HTTP {(int)response.StatusCode}.");
    }

    private static async void AcceptDialogAsync(object? sender, IDialog dialog)
    {
        await dialog.AcceptAsync();
    }

    private static string NewSku(string scenario)
    {
        return $"E2E-{scenario}-{Guid.NewGuid():N}";
    }
}
