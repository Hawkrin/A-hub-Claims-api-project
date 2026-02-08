namespace ASP.Claims.AuditWorker.Repositories;

using ASP.Claims.AuditWorker.Interfaces;
using ASP.Claims.AuditWorker.Models;
using System.Collections.Concurrent;

public class InMemoryAuditRepository(ILogger<InMemoryAuditRepository> logger) : IAuditRepository
{
    private readonly ConcurrentBag<AuditEntry> _auditLog = [];
    private readonly ILogger<InMemoryAuditRepository> _logger = logger;

    public Task SaveAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        _auditLog.Add(entry);
        
        _logger.LogDebug("Audit entry saved to in-memory store: {EventType} for ClaimId {ClaimId} (Total: {Count})", 
            entry.EventType, entry.ClaimId, _auditLog.Count);
        
        return Task.CompletedTask;
    }

    public Task<IEnumerable<AuditEntry>> GetByClaimIdAsync(Guid claimId, CancellationToken cancellationToken = default)
    {
        var entries = _auditLog
            .Where(e => e.ClaimId == claimId)
            .OrderByDescending(e => e.Timestamp)
            .ToList();

        return Task.FromResult<IEnumerable<AuditEntry>>(entries);
    }

    public Task<IEnumerable<AuditEntry>> GetRecentAsync(int count = 100, CancellationToken cancellationToken = default)
    {
        var entries = _auditLog
            .OrderByDescending(e => e.Timestamp)
            .Take(count)
            .ToList();

        return Task.FromResult<IEnumerable<AuditEntry>>(entries);
    }
}
