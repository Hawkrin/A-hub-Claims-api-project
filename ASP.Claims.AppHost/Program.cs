var builder = DistributedApplication.CreateBuilder(args);

// Note: Using standalone Cosmos DB Emulator (must be started manually before running Aspire)
// Start the emulator via: .\scripts\Start-CosmosEmulator.ps1
// Or from Start Menu: "Azure Cosmos DB Emulator"

// Add Redis for pub/sub messaging (local dev container)
var redis = builder.AddRedis("serviceBus");

// API Project - connects to manually-started standalone Cosmos DB Emulator at https://localhost:8081
var api = builder.AddProject<Projects.ASP_Claims_API>("claims-api")
    .WithEnvironment("ASPIRE_ALLOW_UNSECURED_TRANSPORT", "true")
    .WithHttpEndpoint(port: 5021, name: "http-api")
    .WithExternalHttpEndpoints()
    .WithEnvironment("ClaimsApiBaseUrlPath", "http://localhost:5021/")
    .WithReference(redis);

// Notifications Sender - subscribes to claim events for notifications
builder.AddProject<Projects.ASP_Claims_NotificationsWorker>("notifications-sender")
    .WithReference(redis);

// Audit Logger - subscribes to claim events for audit logging
// Connects to Cosmos DB Emulator for persistent audit trail
builder.AddProject<Projects.ASP_Claims_AuditWorker>("audit-logger")
    .WithReference(redis)
    .WithEnvironment("ASPIRE_ALLOW_UNSECURED_TRANSPORT", "true");

builder.Build().Run();
