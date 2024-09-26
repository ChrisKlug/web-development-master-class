using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebDevMasterClass.Services.Products.Client;

namespace WebDevMasterClass.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController(IProductsClient productsClient) : ControllerBase
{
    [HttpGet("featured")]
    public async Task<Ok<Product[]>> GetFeaturedProducts()
        => TypedResults.Ok(await productsClient.GetFeaturedProducts());

    [HttpGet("{productId:int}")]
    public async Task<Results<NotFound, Ok<Product>>> GetProduct(int productId)
    {
        var product = await productsClient.GetProduct(productId);

        return product is not null ? TypedResults.Ok(product) : TypedResults.NotFound();
    }
}