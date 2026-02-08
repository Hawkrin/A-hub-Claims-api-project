using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Application.Services;
using Microsoft.Azure.Cosmos;

namespace ASP.Claims.API.Extensions;

/// <summary>
/// Extension methods for Cosmos DB initialization and seeding
/// </summary>
public static class CosmosDbExtensions
{
    /// <summary>
    /// Initializes Cosmos DB containers and optionally seeds data in Development environment
    /// </summary>
    public static async Task InitializeCosmosDbAsync(
        this WebApplication app,
        CancellationToken cancellationToken = default)
    {
        var cosmosClient = app.Services.GetService<CosmosClient>();
        
        if (cosmosClient == null)
        {
            app.Logger.LogInformation("Using in-memory repositories (CosmosClient not registered)");
            return;
        }

        var dbName = app.Configuration["CosmosDb:DatabaseName"] ?? "ClaimsDb";
        var claimsContainer = app.Configuration["CosmosDb:Containers:Claims"] ?? "Claims";
        var usersContainer = app.Configuration["CosmosDb:Containers:Users"] ?? "Users";

        try
        {
            // Initialize database and containers
            await CosmosDbInitializer.InitializeAsync(
                cosmosClient, 
                dbName, 
                claimsContainer, 
                usersContainer, 
                cancellationToken);
            
            app.Logger.LogInformation(
                "CosmosDB initialized successfully - Database: {DatabaseName}, Containers: {Claims}, {Users}", 
                dbName, claimsContainer, usersContainer);

            // Seed data in Development only
            if (app.Environment.IsDevelopment())
            {
                await SeedDevelopmentDataAsync(app, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "Failed to initialize or seed CosmosDB - will retry on first use");
        }
    }

    /// <summary>
    /// Seeds initial data for development and testing
    /// </summary>
    private static async Task SeedDevelopmentDataAsync(
        WebApplication app, 
        CancellationToken cancellationToken)
    {
        using var scope = app.Services.CreateScope();
        
        var claimRepository = scope.ServiceProvider.GetRequiredService<IClaimRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CosmosDbSeeder>>();
        
        var seeder = new CosmosDbSeeder(claimRepository, userRepository, logger);
        
        await seeder.SeedAsync(cancellationToken);
        
        app.Logger.LogInformation("CosmosDB seeding completed");
    }
}
