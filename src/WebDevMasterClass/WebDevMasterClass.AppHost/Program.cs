using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var ui = builder.AddDockerfile("ui", "../../../_resources/ui")
                .WithHttpEndpoint(targetPort: 80, env: "PORT");

builder.AddProject<Projects.WebDevMasterClass_Services_Products>("products")
        .WithEnvironment("ConnectionStrings__Sql", builder.Configuration.GetConnectionString("Products"));

builder.AddProject<Projects.WebDevMasterClass_Web>("web", "aspire")
        .WithReference(ui.GetEndpoint("http"))
        .WithExternalHttpEndpoints();

builder.Build().Run();
