using Microsoft.Azure.Cosmos;

namespace ASP.Claims.API.Application.Services;

public static class CosmosDbInitializer
{
    public static async Task InitializeAsync(
        CosmosClient cosmosClient,
        string databaseName,
        string claimsContainerName,
        string usersContainerName,
        CancellationToken cancellationToken = default)
    {
        var dbResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName, cancellationToken: cancellationToken);
        var database = dbResponse.Database;

        await database.CreateContainerIfNotExistsAsync(
            new ContainerProperties(claimsContainerName, partitionKeyPath: "/id"),
            cancellationToken: cancellationToken);

        await database.CreateContainerIfNotExistsAsync(
            new ContainerProperties(usersContainerName, partitionKeyPath: "/id"),
            cancellationToken: cancellationToken);
    }
}
