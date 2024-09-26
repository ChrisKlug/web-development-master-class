using System.Net.Http.Json;
using System.Text.Json;

namespace WebDevMasterClass.Services.Products.Client;

internal class HttpProductsClient(HttpClient httpClient) : IProductsClient
{
    public Task<Product[]> GetFeaturedProducts()
        => httpClient.GetFromJsonAsync<Product[]>("api/products/featured")!;

    public async Task<Product?> GetProduct(int id)
    {
        var response = await httpClient.GetAsync("api/products/" + id);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        return JsonSerializer.Deserialize<Product>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}
