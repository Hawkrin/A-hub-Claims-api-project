var builder = DistributedApplication.CreateBuilder(args);

// Note: Using standalone Cosmos DB Emulator (must be started manually before running Aspire)
// Start the emulator via: .\scripts\Start-CosmosEmulator.ps1
// Or from Start Menu: "Azure Cosmos DB Emulator"

// API Project - connects to manually-started standalone Cosmos DB Emulator at https://localhost:8081
var api = builder.AddProject<Projects.ASP_Claims_API>("api")
    .WithEnvironment("ASPIRE_ALLOW_UNSECURED_TRANSPORT", "true")
    .WithHttpEndpoint(port: 5020, name: "http-api")
    .WithEnvironment("ClaimsApiBaseUrlPath", "http://localhost:5020/");

builder.Build().Run();
