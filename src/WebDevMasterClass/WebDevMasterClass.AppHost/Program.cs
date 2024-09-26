using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var ui = builder.AddDockerfile("ui", "../../../_resources/ui")
                .WithHttpEndpoint(targetPort: 80, env: "PORT");

var idSrv = builder.AddContainer("identityserver", "identity-server")
                    .WithBindMount(Path.Combine(Environment.CurrentDirectory, "../../../_resources/"), "/devcert", true)
                    .WithHttpsEndpoint(targetPort: 8081)
                    .WithEnvironment("ASPNETCORE_URLS", "https://*:8081")
                    .WithEnvironment("Kestrel__Certificates__Default__Path", "/devcert/ssl-cert.pfx")
                    .WithEnvironment("Kestrel__Certificates__Default__Password", "P@ssw0rd123!")
                    .WithExternalHttpEndpoints();

var products = builder.AddProject<Projects.WebDevMasterClass_Services_Products>("products")
        .WithEnvironment("ConnectionStrings__Sql", builder.Configuration.GetConnectionString("Products"));

builder.AddProject<Projects.WebDevMasterClass_Web>("web", "aspire")
        .WithEnvironment("IdentityServer__Url", idSrv.GetEndpoint("https"))
        .WithReference(ui.GetEndpoint("http"))
        .WithReference(products)
        .WithHttpEndpoint(env: "DashboardPort")
        .WithExternalHttpEndpoints();

builder.Build().Run();
