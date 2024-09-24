using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var ui = builder.AddDockerfile("ui", "../../../_resources/ui")
                .WithHttpEndpoint(targetPort: 80, env: "PORT");

var products = builder.AddProject<Projects.WebDevMasterClass_Services_Products>("products")
        .WithEnvironment("ConnectionStrings__Sql", builder.Configuration.GetConnectionString("Products"));

var idsrv = builder.AddContainer("identityserver", "identity-server")
                    .WithBindMount(Path.Combine(Environment.CurrentDirectory, "../../../_resources"), "/devcert", true)
                    .WithEnvironment("ASPNETCORE_URLS", "https://*:8081")
                    .WithEnvironment("Kestrel__Certificates__Default__Path", "/devcert/ssl-cert.pfx")
                    .WithEnvironment("Kestrel__Certificates__Default__Password", "P@ssw0rd123!")
                    .WithHttpsEndpoint(targetPort: 8081)
                    .WithExternalHttpEndpoints();

var orders = builder.AddProject<Projects.WebDevMasterClass_Services_Orders>("orders")
        .WithEnvironment("ConnectionStrings__Sql", builder.Configuration.GetConnectionString("Orders"));

builder.AddProject<Projects.WebDevMasterClass_Web>("web", "aspire")
        .WithReference(ui.GetEndpoint("http"))
        .WithEnvironment("IdentityServer__Url", idsrv.GetEndpoint("https"))
        .WithReference(products)
        .WithReference(orders)
        .WithHttpEndpoint(env: "DashboardPort")
        .WithExternalHttpEndpoints();


builder.Build().Run();
