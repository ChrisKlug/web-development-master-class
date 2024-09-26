using Microsoft.Extensions.FileProviders;
using WebDevMasterClass.Services.Products.Client;
using WebDevMasterClass.Web.Models;
using WebDevMasterClass.Web.ShoppingCart;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHttpForwarderWithServiceDiscovery();

builder.Services.AddControllers();

builder.Services.AddOrleans(silo =>
{
    silo.UseLocalhostClustering();
    silo.AddMemoryGrainStorageAsDefault();
    if (Environment.GetEnvironmentVariable("DashboardPort") is not null)
    {
        silo.UseDashboard(options =>
        {
            options.Port = int.Parse(Environment.GetEnvironmentVariable("DashboardPort")!);
        });
    }
});

builder.Services.AddProductsClient(options =>
{
    options.BaseUrl = "https://products";
});

var app = builder.Build();

app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = "/images/products",
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "../../_resources/productimages"))
});

app.UseRouting();

app.MapDefaultEndpoints();

app.MapPost("/api/shopping-cart", async (AddShoppingCartItemModel model, HttpContext ctx, 
                                         IProductsClient productsClient, IGrainFactory grainFactory) => {
    string cartId;
    if (ctx.Request.Cookies.ContainsKey("ShoppingCartId"))
    {
        cartId = ctx.Request.Cookies["ShoppingCartId"]!;
    }
    else
    {
        var rnd = new Random();
        cartId = new string(Enumerable.Range(0, 30)
                            .Select(x => (char)rnd.Next(65, 90))
                            .ToArray());
        ctx.Response.Cookies.Append("ShoppingCartId", cartId);
    }
    var product = await productsClient.GetProduct(model.ProductId);
    if (product is null)
    {
        return Results.BadRequest();
    }
    var grain = grainFactory.GetGrain<IShoppingCartGrain>(cartId);
    await grain.AddItem(new ShoppingCartItem
    {
        ProductId = product.Id,
        ProductName = product.Name,
        Price = product.Price,
        Count = model.Count
    });

    return Results.Ok(await grain.GetItems());
});
app.MapGet("api/shopping-cart", async (HttpContext ctx, IGrainFactory grainFactory) =>
{
    if (ctx.Request.Cookies.ContainsKey("ShoppingCartId"))
    {
        var grain = grainFactory.GetGrain<IShoppingCartGrain>(ctx.Request.Cookies["ShoppingCartId"]);
        return Results.Ok(await grain.GetItems());
    }
    return Results.Ok(Array.Empty<ShoppingCartItem>());
});

app.MapControllers();

app.Map("/api/{**catch-all}", (HttpContext ctx) => {
    ctx.Response.StatusCode = 404;
});

app.MapForwarder("/{**catch-all}", "http://ui");

app.Run();

public partial class Program { }