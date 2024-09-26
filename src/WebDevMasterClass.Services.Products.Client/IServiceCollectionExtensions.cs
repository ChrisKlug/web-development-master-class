using Microsoft.Extensions.DependencyInjection;

namespace WebDevMasterClass.Services.Products.Client;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddProductsClient(this IServiceCollection services, Action<ProductsClientOptions> config)
    {
        var options = new ProductsClientOptions();
        config(options);

        if (options.BaseUrl is null)
            throw new Exception("BaseUrl is missing");

        services.AddHttpClient<IProductsClient, HttpProductsClient>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        return services;
    }
}

public class ProductsClientOptions
{
    public string? BaseUrl { get; set; }
}
