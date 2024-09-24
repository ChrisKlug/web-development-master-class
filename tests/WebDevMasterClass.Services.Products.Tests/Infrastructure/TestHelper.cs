using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebDevMasterClass.Services.Products.Data;
using WebDevMasterClass.Services.Products.Tests.Data;

namespace WebDevMasterClass.Services.Products.Tests.Infrastructure;

public static class TestHelper
{
    public static async Task ExecuteTest(Func<SqlCommand, Task> dbSetup, Func<HttpClient, Task> test)
    {
        var app = new WebApplicationFactory<Program>()
                            .WithWebHostBuilder(builder =>
                            {
                                builder.UseEnvironment("IntegrationTesting");

                                builder.ConfigureTestServices(services => {
                                    var dbDescriptor = services.First(x => x.ServiceType == typeof(ProductsContext));
                                    var optionsDescriptor = services.First(x => x.ServiceType == typeof(DbContextOptions<ProductsContext>));

                                    services.Remove(dbDescriptor);
                                    services.Remove(optionsDescriptor);

                                    services.AddDbContext<ProductsContext>((services, options) =>
                                    {
                                        var config = services.GetRequiredService<IConfiguration>();
                                        options.UseSqlServer(config.GetConnectionString("Sql"));
                                    }, ServiceLifetime.Singleton);
                                });
                            });

        var ctx = app.Services.GetRequiredService<ProductsContext>();

        using var transaction = ctx.Database.BeginTransaction();

        var conn = ctx.Database.GetDbConnection();
        var cmd = (SqlCommand)conn.CreateCommand();
        cmd.Transaction = (SqlTransaction)transaction.GetDbTransaction();

        await dbSetup(cmd);

        var client = app.CreateClient();

        await test(client);
    }
}
