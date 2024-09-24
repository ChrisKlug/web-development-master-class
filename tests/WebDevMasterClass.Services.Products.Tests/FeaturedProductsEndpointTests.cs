using Newtonsoft.Json.Linq;
using System.Net;
using WebDevMasterClass.Services.Products.Tests.Data;
using WebDevMasterClass.Services.Products.Tests.Infrastructure;

namespace WebDevMasterClass.Services.Products.Tests
{
    public class FeaturedProductsEndpointTests
    {
        //[Fact]
        //public async Task GET_Returns_HTTP_200_and_all_products_marked_as_featured()
        //{
        //    var app = new WebApplicationFactory<Program>()
        //                    .WithWebHostBuilder(builder =>
        //                    {
        //                        builder.UseEnvironment("IntegrationTesting");
        //                        //builder.ConfigureAppConfiguration((ctx, config) => {
        //                        //    config.AddInMemoryCollection([
        //                        //        new KeyValuePair<string, string?>(
        //                        //            "ConnectionStrings:Sql",
        //                        //            "Server=.;Database=WebDevelopmentMasterClass.Products.Tests;User Id=sa;Password=MyVerySecretPassw0rd;Encrypt=Yes;Trust Server Certificate=Yes;"
        //                        //        )
        //                        //    ]);
        //                        //});

        //                        builder.ConfigureTestServices(services => {
        //                            var dbDescriptor = services.First(x => x.ServiceType == typeof(ProductsContext));
        //                            var optionsDescriptor = services.First(x => x.ServiceType == typeof(DbContextOptions<ProductsContext>));

        //                            services.Remove(dbDescriptor);
        //                            services.Remove(optionsDescriptor);

        //                            services.AddDbContext<ProductsContext>((services, options) =>
        //                            {
        //                                var config = services.GetRequiredService<IConfiguration>();
        //                                options.UseSqlServer(config.GetConnectionString("Sql"));
        //                            }, ServiceLifetime.Singleton);
        //                        });
        //                    });

        //    var ctx = app.Services.GetRequiredService<ProductsContext>();

        //    using var transaction = ctx.Database.BeginTransaction();

        //    var conn = ctx.Database.GetDbConnection();
        //    var cmd = (SqlCommand)conn.CreateCommand();
        //    cmd.Transaction = (SqlTransaction)transaction.GetDbTransaction();

        //    await cmd.AddProduct("Product 1", "Description 1", 100m, true);
        //    await cmd.AddProduct("Product 2", "Description 2", 200m, true);
        //    await cmd.AddProduct("Product 3", "Description 3", 300m, true);
        //    await cmd.AddProduct("Product", "Description", 50m, false);

        //    var client = app.CreateClient();

        //    var response = await client.GetAsync("/api/products/featured");

        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        //    var products = JArray.Parse(await response.Content.ReadAsStringAsync());

        //    Assert.Equal(3, products.Count);

        //    Assert.Contains(products, x => x.Value<string>("name") == "Product 1");
        //    Assert.Contains(products, x => x.Value<string>("name") == "Product 2");
        //    Assert.Contains(products, x => x.Value<string>("name") == "Product 3");
        //}
        [Fact]
        public Task GET_Returns_HTTP_200_and_all_products_marked_as_featured()
            => TestHelper.ExecuteTest(
                dbSetup: async cmd => {
                    await cmd.AddProduct("Product 1", "Description 1", 100m, true, "Product1");
                    await cmd.AddProduct("Product 2", "Description 2", 200m, true, "Product2");
                    await cmd.AddProduct("Product 3", "Description 3", 300m, true, "Product3");
                    await cmd.AddProduct("Product", "Description", 50m, false, "Product");
                },
                test: async client => {
                    var response = await client.GetAsync("/api/products/featured");

                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    var products = JArray.Parse(await response.Content.ReadAsStringAsync());

                    Assert.Equal(3, products.Count);

                    Assert.Contains(products, x => x.Value<string>("name") == "Product 1");
                    Assert.Contains(products, x => x.Value<string>("name") == "Product 2");
                    Assert.Contains(products, x => x.Value<string>("name") == "Product 3");
                });
    }
}