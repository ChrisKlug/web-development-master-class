using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebDevMasterClass.Services.Products.Data;

namespace WebDevMasterClass.Services.Products.Tests.Infrastructure;

public static class TestHelper
{
    public static async Task ExecuteTest(Func<SqlCommand, Task> dbSetup, 
                                        Func<HttpClient, Task> test)
    {
        var app = new WebApplicationFactory<Program>()
                        .WithWebHostBuilder(builder =>
                        {
                            //builder.ConfigureAppConfiguration((context, config) =>
                            //{
                            //    config.AddInMemoryCollection([
                            //        new KeyValuePair<string, string?>("ConnectionStrings:Sql", "Server=.;Database=WebDevMasterClass.Products.Tests;User Id=sa;Password=MyVerySecretPassw0rd;Encrypt=Yes;Trust Server Certificate=Yes;"),
                            //    ]);
                            //});
                            builder.UseEnvironment("IntegrationTesting");

                            builder.ConfigureTestServices(services =>
                            {
                                var dbDescriptor = services.First(x => x.ServiceType == typeof(ProductsContext));
                                var dbOptionsDescriptor = services.First(x => x.ServiceType == typeof(DbContextOptions<ProductsContext>));

                                services.Remove(dbDescriptor);
                                services.Remove(dbOptionsDescriptor);

                                services.AddDbContext<ProductsContext>((services, options) =>
                                {
                                    var config = services.GetRequiredService<IConfiguration>();
                                    options.UseSqlServer(config.GetConnectionString("Sql"));
                                }, ServiceLifetime.Singleton);
                            });
                        });

        var ctx = app.Services.GetRequiredService<ProductsContext>();
        using (var transaction = ctx.Database.BeginTransaction())
        using (var conn = ctx.Database.GetDbConnection())
        {
            var cmd = (SqlCommand)conn.CreateCommand();
            cmd.Transaction = (SqlTransaction)transaction.GetDbTransaction();

            await dbSetup(cmd);

            var client = app.CreateClient();

            await test(client);
        }
    }
}
