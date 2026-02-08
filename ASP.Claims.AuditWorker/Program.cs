using ASP.Claims.AuditWorker;
using ASP.Claims.AuditWorker.Interfaces;
using ASP.Claims.AuditWorker.Repositories;
using Microsoft.Azure.Cosmos;

var builder = Host.CreateApplicationBuilder(args);

// Add Aspire service defaults (telemetry, health checks)
builder.AddServiceDefaults();

// Add Redis connection for pub/sub
builder.AddRedisClient("ServiceBus");

// Configure Audit Repository based on environment
if (builder.Environment.IsProduction() || builder.Configuration.GetValue<bool>("UseCosmosDbAudit"))
{
    try
    {
        // Use Cosmos DB for audit logs (SEPARATE DATABASE for compliance)
        var connectionString = builder.Configuration.GetConnectionString("AuditDb");
        
        if (!string.IsNullOrEmpty(connectionString))
        {
            builder.Services.AddSingleton<CosmosClient>(sp =>
            {
                var cosmosClientOptions = new CosmosClientOptions
                {
                    SerializerOptions = new CosmosSerializationOptions
                    {
                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                    },
                    HttpClientFactory = () => new HttpClient(new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                    }),
                    ConnectionMode = ConnectionMode.Gateway,
                    LimitToEndpoint = true
                };
                
                return new CosmosClient(connectionString, cosmosClientOptions);
            });
            
            builder.Services.AddSingleton<IAuditRepository>(sp =>
            {
                var cosmosClient = sp.GetRequiredService<CosmosClient>();
                var config = sp.GetRequiredService<IConfiguration>();
                var logger = sp.GetRequiredService<ILogger<CosmosDbAuditRepository>>();
                
                // SEPARATE DATABASE for audit compliance
                var databaseName = config["CosmosDb:DatabaseName"] ?? "AuditDb";
                var containerName = config["CosmosDb:Containers:AuditLogs"] ?? "AuditLogs";
                
                var container = cosmosClient.GetContainer(databaseName, containerName);
                
                return new CosmosDbAuditRepository(container, logger);
            });
            
            Console.WriteLine("? AuditWorker configured to use SEPARATE Cosmos DB (AuditDb) for compliance");
        }
        else
        {
            Console.WriteLine("??  Cosmos DB connection string not found, falling back to in-memory");
            builder.Services.AddSingleton<IAuditRepository, InMemoryAuditRepository>();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"??  Failed to configure Cosmos DB, falling back to in-memory: {ex.Message}");
        builder.Services.AddSingleton<IAuditRepository, InMemoryAuditRepository>();
    }
}
else
{
    // Development: Use in-memory storage
    builder.Services.AddSingleton<IAuditRepository, InMemoryAuditRepository>();
    
    Console.WriteLine("??  AuditWorker configured to use IN-MEMORY storage (Development mode)");
}

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

// Initialize Cosmos DB audit container if using Cosmos
if (builder.Environment.IsProduction() || builder.Configuration.GetValue<bool>("UseCosmosDbAudit"))
{
    var cosmosClient = host.Services.GetService<CosmosClient>();
    if (cosmosClient != null)
    {
        var config = host.Services.GetRequiredService<IConfiguration>();
        var databaseName = config["CosmosDb:DatabaseName"] ?? "AuditDb";
        var containerName = config["CosmosDb:Containers:AuditLogs"] ?? "AuditLogs";
        
        try
        {
            // Create SEPARATE audit database
            var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
            
            await database.Database.CreateContainerIfNotExistsAsync(
                new ContainerProperties
                {
                    Id = containerName,
                    PartitionKeyPath = "/partitionKey",
                    DefaultTimeToLive = -1 // Never expire audit logs (compliance requirement)
                },
                throughput: 400 // Low throughput for audit logs (cost optimization)
            );
            
            Console.WriteLine($"? Separate audit database initialized: {databaseName}/{containerName}");
            Console.WriteLine($"   ? Immutable audit trail");
            Console.WriteLine($"   ? Optimized for compliance (400 RU/s)");
            Console.WriteLine($"   ? No TTL (permanent retention)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"??  Failed to initialize Cosmos DB audit container: {ex.Message}");
        }
    }
}

host.Run();
