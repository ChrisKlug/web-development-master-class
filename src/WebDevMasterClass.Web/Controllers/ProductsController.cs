using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebDevMasterClass.Services.Products.Client;

namespace WebDevMasterClass.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController(IProductsClient productsClient) : ControllerBase
{
    [HttpGet("{productId:int}")]
    public async Task<Results<NotFound, Ok<Product>>> GetProducts(int productId)
    {
        var product = await productsClient.GetProduct(productId);

        return product != null ? TypedResults.Ok(product) : TypedResults.NotFound();
    }

    [HttpGet("featured")]
    public async Task<Ok<Product[]>> GetfeaturedProducts()
        => TypedResults.Ok(await productsClient.GetFeaturedProducts());
}
