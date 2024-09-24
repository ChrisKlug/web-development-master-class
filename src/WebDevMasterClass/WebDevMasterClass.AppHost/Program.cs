var builder = DistributedApplication.CreateBuilder(args);

var ui = builder.AddDockerfile("ui", "../../../_resources/ui")
                .WithHttpEndpoint(targetPort: 80, env: "PORT");

builder.AddProject<Projects.WebDevMasterClass_Web>("web", "aspire")
        .WithReference(ui.GetEndpoint("http"))
        .WithExternalHttpEndpoints();

builder.Build().Run();
