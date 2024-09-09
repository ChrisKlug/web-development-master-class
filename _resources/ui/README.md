# WebDevMasterClass.UI is the UI for the application

To keep the workshop a bit shorter, the UI is only added as a pre-built Docker image. However, this project includes the source code for the image that is used.

## Building the image

To build the image, you just have to run the following command

```
docker build -t webdevmasterclass-ui .
```

And then add it to the AppHost using the following code

```csharp
var ui = builder.AddContainer("ui", "webdevmasterclass-ui")
                .WithHttpEndpoint(targetPort: 80, env: "PORT");
```

## Running as NPM App

To run the .NET Aspire solution with the UI project instead of the image. You can use the following code

```csharp
var ui = builder.AddNpmApp("ui", "../../resources/WebDevMasterClass.UI")
   .WithEnvironment("BROWSER", "none")
   .WithEnvironment("WDS_SOCKET_PORT", "7278")
   .WithHttpEndpoint(env: "PORT");
```

The `BROWSER` environment variable just makes sure that no browser is opened when running the project. Abd the `WDS_SOCKET_PORT` is the port that the Webpack Dev Server is running on. This is used to make sure that the front end can open a websocket to the application through the .NET Aspire networking.

Note: This requires the `Aspire.Hosting.NodeJs` NuGet package to be installed, as the `AddNpmApp()` method is in that package.