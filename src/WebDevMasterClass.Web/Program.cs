using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Net;
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

builder.Services.AddGrpcClient<WebDevMasterClass.Services.Orders.gRPC.OrdersService.OrdersServiceClient>(options =>
{
    options.Address = new Uri("https://orders");
});

var auth = builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie();
if (!builder.Environment.IsEnvironment("IntegrationTesting"))
{
    auth.AddOpenIdConnect(options =>
    {
        options.Authority = builder.Configuration["IdentityServer:Url"];
        // options.RequireHttpsMetadata = false;

        options.ClientId = "interactive.mvc.sample";
        options.ClientSecret = "secret";

        // code flow + PKCE (PKCE is turned on by default)
        options.ResponseType = "code";
        options.UsePkce = true;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("scope1");
        options.Scope.Add("offline_access");

        // not mapped by default
        options.ClaimActions.MapJsonKey("website", "website");

        // keeps id_token smaller
        options.GetClaimsFromUserInfoEndpoint = true;
        // save tokens in cookie
        options.SaveTokens = true;
        // disable MS auto claim type renaming
        options.MapInboundClaims = false;
        //Disable x-client-SKU and x-client-ver headers (security issue)
        options.DisableTelemetry = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "name",
            RoleClaimType = "role"
        };

        options.Events.OnRedirectToIdentityProvider = context =>
        {
            if (context.HttpContext.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.HandleResponse();
            }
            return Task.CompletedTask;
        };
    });
}

var app = builder.Build();

app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = "/images/products",
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "../../_resources/productimages"))
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

app.MapGet("/api/me", (ClaimsPrincipal user) => {
    return Results.Ok(user.Identity!.Name);
}).RequireAuthorization();

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