using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var ui = builder.AddDockerfile("ui", "../../../_resources/ui")
                .WithHttpEndpoint(targetPort: 80, env: "PORT");

builder.Build().Run();
