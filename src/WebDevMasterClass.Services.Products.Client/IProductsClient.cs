namespace WebDevMasterClass.Services.Products.Client;

public interface IProductsClient
{
    Task<Product?> GetProduct(int id);
    Task<Product[]> GetFeaturedProducts();
}

public record Product(int Id, string Name, string Description, decimal Price, bool IsFeatured, string ThumbnailUrl, string ImageUrl);
