using ASP.Claims.API.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

namespace ASP.Claims.API.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuditController : ControllerBase
{
    private readonly CosmosClient _cosmosClient;
    private readonly ILogger<AuditController> _logger;
    private readonly IConfiguration _configuration;

    public AuditController(CosmosClient cosmosClient, ILogger<AuditController> logger, IConfiguration configuration)
    {
        _cosmosClient = cosmosClient;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Get all audit logs (paginated)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<AuditLogDto>), 200)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var databaseName = _configuration["AuditDb:DatabaseName"] ?? "AuditDb";
            var containerName = _configuration["AuditDb:ContainerName"] ?? "AuditLogs";
            
            var container = _cosmosClient.GetContainer(databaseName, containerName);
            
            var query = new QueryDefinition(
                $"SELECT TOP {pageSize} * FROM c ORDER BY c.timestamp DESC OFFSET {(page - 1) * pageSize} LIMIT {pageSize}");
            
            var results = new List<AuditLogDto>();
            using var iterator = container.GetItemQueryIterator<AuditLogDto>(query);
            
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                results.AddRange(response);
            }
            
            _logger.LogInformation("Retrieved {Count} audit logs (page {Page})", results.Count, page);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audit logs");
            return StatusCode(500, new { error = "Failed to retrieve audit logs" });
        }
    }

    /// <summary>
    /// Get audit logs for a specific claim
    /// </summary>
    [HttpGet("claim/{claimId}")]
    [ProducesResponseType(typeof(List<AuditLogDto>), 200)]
    public async Task<IActionResult> GetAuditLogsByClaim(
        Guid claimId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var databaseName = _configuration["AuditDb:DatabaseName"] ?? "AuditDb";
            var containerName = _configuration["AuditDb:ContainerName"] ?? "AuditLogs";
            
            var container = _cosmosClient.GetContainer(databaseName, containerName);
            
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.claimId = @claimId ORDER BY c.timestamp DESC")
                .WithParameter("@claimId", claimId);
            
            var results = new List<AuditLogDto>();
            using var iterator = container.GetItemQueryIterator<AuditLogDto>(query);
            
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                results.AddRange(response);
            }
            
            _logger.LogInformation("Retrieved {Count} audit logs for claim {ClaimId}", results.Count, claimId);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audit logs for claim {ClaimId}", claimId);
            return StatusCode(500, new { error = "Failed to retrieve audit logs" });
        }
    }

    /// <summary>
    /// Get audit statistics
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(AuditStatsDto), 200)]
    public async Task<IActionResult> GetAuditStats(CancellationToken cancellationToken = default)
    {
        try
        {
            var databaseName = _configuration["AuditDb:DatabaseName"] ?? "AuditDb";
            var containerName = _configuration["AuditDb:ContainerName"] ?? "AuditLogs";
            
            var container = _cosmosClient.GetContainer(databaseName, containerName);
            
            // Total count
            var countQuery = new QueryDefinition("SELECT VALUE COUNT(1) FROM c");
            var countIterator = container.GetItemQueryIterator<int>(countQuery);
            var countResponse = await countIterator.ReadNextAsync(cancellationToken);
            var totalCount = countResponse.FirstOrDefault();
            
            // Count by event type
            var eventTypeQuery = new QueryDefinition(
                "SELECT c.eventType, COUNT(1) as count FROM c GROUP BY c.eventType");
            var eventTypeIterator = container.GetItemQueryIterator<dynamic>(eventTypeQuery);
            var eventTypeCounts = new Dictionary<string, int>();
            
            while (eventTypeIterator.HasMoreResults)
            {
                var response = await eventTypeIterator.ReadNextAsync(cancellationToken);
                foreach (var item in response)
                {
                    eventTypeCounts[item.eventType.ToString()] = (int)item.count;
                }
            }
            
            var stats = new AuditStatsDto
            {
                TotalAuditEntries = totalCount,
                EventTypeCounts = eventTypeCounts,
                LastUpdated = DateTime.UtcNow
            };
            
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audit statistics");
            return StatusCode(500, new { error = "Failed to retrieve audit statistics" });
        }
    }

    /// <summary>
    /// Get recent audit activity (last 24 hours)
    /// </summary>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(List<AuditLogDto>), 200)]
    public async Task<IActionResult> GetRecentAuditLogs(
        [FromQuery] int hours = 24,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var databaseName = _configuration["AuditDb:DatabaseName"] ?? "AuditDb";
            var containerName = _configuration["AuditDb:ContainerName"] ?? "AuditLogs";
            
            var container = _cosmosClient.GetContainer(databaseName, containerName);
            
            var since = DateTime.UtcNow.AddHours(-hours);
            
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.timestamp >= @since ORDER BY c.timestamp DESC")
                .WithParameter("@since", since);
            
            var results = new List<AuditLogDto>();
            using var iterator = container.GetItemQueryIterator<AuditLogDto>(query);
            
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                results.AddRange(response);
            }
            
            _logger.LogInformation("Retrieved {Count} audit logs from last {Hours} hours", results.Count, hours);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve recent audit logs");
            return StatusCode(500, new { error = "Failed to retrieve recent audit logs" });
        }
    }
}
