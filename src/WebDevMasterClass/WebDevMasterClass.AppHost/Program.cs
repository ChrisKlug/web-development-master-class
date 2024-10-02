var builder = DistributedApplication.CreateBuilder(args);

var ui = builder.AddDockerfile("ui", "../../../_resources/ui-svelte")
                .WithHttpEndpoint(targetPort: 80, env: "PORT");

builder.Build().Run();
