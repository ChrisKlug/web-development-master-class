extern alias SERVER;

using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebDevMasterClass.Services.Orders.gRPC;
using OrdersServer = SERVER::WebDevMasterClass.Services.Orders;

public static class TestHelper
{
    public static async Task ExecuteTest(Func<OrdersService.OrdersServiceClient, Task> test, Func<SqlCommand, Task>? dbConfig = null, Func<SqlCommand, Task>? validateDb = null)
    {
        var app = new WebApplicationFactory<SERVER::Program>()
                        .WithWebHostBuilder(builder =>
                        {
                            builder.UseEnvironment("IntegrationTesting");

                            builder.ConfigureTestServices(services =>
                            {
                                var dbDescriptor = services.First(x => x.ServiceType == typeof(OrdersServer.Data.OrdersContext));
                                services.Remove(dbDescriptor);

                                var optionsDescriptor = services.First(x => x.ServiceType == typeof(DbContextOptions<OrdersServer.Data.OrdersContext>));
                                services.Remove(optionsDescriptor);

                                var config = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

                                services.AddDbContext<OrdersServer.Data.OrdersContext>(options =>
                                {
                                    options.UseSqlServer(config.GetConnectionString("Sql"));
                                }, ServiceLifetime.Singleton);
                            });
                        });

        using (var services = app.Services.CreateScope())
        {
            var ctx = services.ServiceProvider.GetRequiredService<OrdersServer.Data.OrdersContext>();
            using (var transaction = ctx.Database.BeginTransaction())
            {
                var conn = ctx.Database.GetDbConnection();
                var cmd = (SqlCommand)conn.CreateCommand();
                cmd.Transaction = (SqlTransaction)transaction.GetDbTransaction();

                if (dbConfig != null)
                    await dbConfig(cmd);

                var options = new GrpcChannelOptions
                {
                    HttpHandler = app.Server.CreateHandler()
                };
                var channel = GrpcChannel.ForAddress(app.Server.BaseAddress, options);
                var client = new OrdersService.OrdersServiceClient(channel);

                await test(client);

                if (validateDb != null)
                    await validateDb(cmd);
            }
        }
    }
}