using Newtonsoft.Json.Linq;
using System.Net;
using WebDevMasterClass.Services.Products.Data;
using WebDevMasterClass.Services.Products.Tests.Data;
using WebDevMasterClass.Testing;

namespace WebDevMasterClass.Services.Products.Tests;

public class FeaturedEndpointsTests
{
    [Fact]
    public Task GET_returns_HTTP_200_and_all_products_marked_as_featured()
        => TestHelper.ForHttp<Program, ProductsContext>().ExecuteTest(dbSetup: async cmd => {
            await cmd.AddProduct("Product 1", "Description 1", 100m, true, "product1");
            await cmd.AddProduct("Product 2", "Description 2", 200m, true, "product2");
            await cmd.AddProduct("Product 3", "Description 3", 300m, true, "product3");
            await cmd.AddProduct("Product 4", "Description 4", 50m, false, "product4");
        }, test: async client => {
            var response = await client.GetAsync("/api/products/featured");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var products = JArray.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal(3, products.Count);

            Assert.Contains(products, x => x.Value<string>("name") == "Product 1");
            Assert.Contains(products, x => x.Value<string>("name") == "Product 2");
            Assert.Contains(products, x => x.Value<string>("name") == "Product 3");
        }); 
    //{
        //var app = new WebApplicationFactory<Program>()
        //                .WithWebHostBuilder(builder =>
        //                {
        //                    //builder.ConfigureAppConfiguration((context, config) =>
        //                    //{
        //                    //    config.AddInMemoryCollection([
        //                    //        new KeyValuePair<string, string?>("ConnectionStrings:Sql", "Server=.;Database=WebDevMasterClass.Products.Tests;User Id=sa;Password=MyVerySecretPassw0rd;Encrypt=Yes;Trust Server Certificate=Yes;"),
        //                    //    ]);
        //                    //});
        //                    builder.UseEnvironment("IntegrationTesting");

        //                    builder.ConfigureTestServices(services =>
        //                    {
        //                        var dbDescriptor = services.First(x => x.ServiceType == typeof(ProductsContext));
        //                        var dbOptionsDescriptor = services.First(x => x.ServiceType == typeof(DbContextOptions<ProductsContext>));

        //                        services.Remove(dbDescriptor);
        //                        services.Remove(dbOptionsDescriptor);

        //                        services.AddDbContext<ProductsContext>((services, options) =>
        //                        {
        //                            var config = services.GetRequiredService<IConfiguration>();
        //                            options.UseSqlServer(config.GetConnectionString("Sql"));
        //                        }, ServiceLifetime.Singleton);
        //                    });
        //                });

        //var ctx = app.Services.GetRequiredService<ProductsContext>();
        //using (var transaction = ctx.Database.BeginTransaction())
        //using (var conn = ctx.Database.GetDbConnection())
        //{
        //    var cmd = (SqlCommand)conn.CreateCommand();
        //    cmd.Transaction = (SqlTransaction)transaction.GetDbTransaction();

        //    await cmd.AddProduct("Product 1", "Description 1", 100m, true, "product1");
        //    await cmd.AddProduct("Product 2", "Description 2", 200m, true, "product2");
        //    await cmd.AddProduct("Product 3", "Description 3", 300m, true, "product3");
        //    await cmd.AddProduct("Product 4", "Description 4", 50m, false, "product4");

        //    var client = app.CreateClient();

        //    var response = await client.GetAsync("/api/products/featured");

        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        //    var products = JArray.Parse(await response.Content.ReadAsStringAsync());

        //    Assert.Equal(3, products.Count);

        //    Assert.Contains(products, x => x.Value<string>("name") == "Product 1");
        //    Assert.Contains(products, x => x.Value<string>("name") == "Product 2");
        //    Assert.Contains(products, x => x.Value<string>("name") == "Product 3");
        //}
    //}
}