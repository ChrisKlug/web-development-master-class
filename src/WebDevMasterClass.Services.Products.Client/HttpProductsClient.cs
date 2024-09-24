using System.Net.Http.Json;

namespace WebDevMasterClass.Services.Products.Client;

internal class HttpProductsClient(HttpClient httpClient) : IProductsClient
{
    public Task<Product[]> GetFeaturedProducts()
        => httpClient.GetFromJsonAsync<Product[]>("/api/products/featured")!;


    public Task<Product?> GetProduct(int productId)
            => httpClient.GetFromJsonAsync<Product>($"/api/products/{productId}");
}
