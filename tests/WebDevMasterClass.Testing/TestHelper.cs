using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebDevMasterClass.Testing;

public class TestHelper
{
    public static TestHelper<TProgram, TDbContext, HttpClient> ForHttp<TProgram, TDbContext>()
                where TProgram : class
                where TDbContext : DbContext
        => new TestHelper<TProgram, TDbContext, HttpClient>(app => app.CreateClient());

    public static TestHelper<TProgram, TDbContext, TClient> ForGrpc<TProgram, TDbContext, TClient>(Action<DbContextOptionsBuilder> dbConfig)
                where TProgram : class
                where TDbContext : DbContext
                where TClient : ClientBase
    {
        return new TestHelper<TProgram, TDbContext, TClient>(app => {
            var options = new GrpcChannelOptions
            {
                HttpHandler = app.Server.CreateHandler()
            };
            var channel = GrpcChannel.ForAddress(app.Server.BaseAddress, options);
            return (TClient)Activator.CreateInstance(typeof(TClient), channel)!;
        },
        dbConfig);
    }
}

public class TestHelper<TProgram, TDbContext, TClient>
                where TProgram : class
                where TDbContext : DbContext
{
    private readonly Func<WebApplicationFactory<TProgram>, TClient> clientCreator;
    private readonly Action<DbContextOptionsBuilder>? configureDb;

    public TestHelper(Func<WebApplicationFactory<TProgram>, TClient> clientCreator, Action<DbContextOptionsBuilder>? configureDb = null)
    {
        this.clientCreator = clientCreator;
        this.configureDb = configureDb;
    }

    public async Task ExecuteTest(
            Func<TClient, Task> test,
            Func<SqlCommand, Task>? dbSetup = null,
            Func<SqlCommand, Task>? validateDb = null
            )
    {
        var app = new WebApplicationFactory<TProgram>()
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
                                var dbDescriptor = services.First(x => x.ServiceType == typeof(TDbContext));
                                var dbOptionsDescriptor = services.First(x => x.ServiceType == typeof(DbContextOptions<TDbContext>));

                                services.Remove(dbDescriptor);
                                services.Remove(dbOptionsDescriptor);

                                services.AddDbContext<TDbContext>((services, options) =>
                                {
                                    var config = services.GetRequiredService<IConfiguration>();
                                    options.UseSqlServer(config.GetConnectionString("Sql"));

                                    if (configureDb is not null)
                                        configureDb(options);

                                }, ServiceLifetime.Singleton);
                            });
                        });

        var ctx = app.Services.GetRequiredService<TDbContext>();
        using (var transaction = ctx.Database.BeginTransaction())
        using (var conn = ctx.Database.GetDbConnection())
        {
            var cmd = (SqlCommand)conn.CreateCommand();
            cmd.Transaction = (SqlTransaction)transaction.GetDbTransaction();

            if (dbSetup is not null)
                await dbSetup(cmd);

            var client = clientCreator(app);

            await test(client);

            if (validateDb != null)
                await validateDb(cmd);
        }
    }
}
