namespace ASP.Claims.AuditWorker.Repositories;

using ASP.Claims.AuditWorker.Interfaces;
using ASP.Claims.AuditWorker.Models;
using Microsoft.Azure.Cosmos;

public class CosmosDbAuditRepository(Container container, ILogger<CosmosDbAuditRepository> logger) : IAuditRepository
{
    private readonly Container _container = container;
    private readonly ILogger<CosmosDbAuditRepository> _logger = logger;

    public async Task SaveAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        try
        {
            await _container.CreateItemAsync(
                entry,
                new PartitionKey(entry.PartitionKey),
                cancellationToken: cancellationToken
            );

            _logger.LogDebug("Audit entry saved to Cosmos DB: {EventType} for ClaimId {ClaimId}", 
                entry.EventType, entry.ClaimId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save audit entry to Cosmos DB: {EventType} for ClaimId {ClaimId}", 
                entry.EventType, entry.ClaimId);
            throw;
        }
    }

    public async Task<IEnumerable<AuditEntry>> GetByClaimIdAsync(Guid claimId, CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.ClaimId = @claimId ORDER BY c.Timestamp DESC")
            .WithParameter("@claimId", claimId);

        var results = new List<AuditEntry>();
        using var iterator = _container.GetItemQueryIterator<AuditEntry>(query);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }

        return results;
    }

    public async Task<IEnumerable<AuditEntry>> GetRecentAsync(int count = 100, CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition($"SELECT TOP {count} * FROM c ORDER BY c.Timestamp DESC");

        var results = new List<AuditEntry>();
        using var iterator = _container.GetItemQueryIterator<AuditEntry>(query);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }

        return results;
    }
}
