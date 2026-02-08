namespace ASP.Claims.AuditWorker.Interfaces;

using ASP.Claims.AuditWorker.Models;

public interface IAuditRepository
{
    Task SaveAsync(AuditEntry entry, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditEntry>> GetByClaimIdAsync(Guid claimId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditEntry>> GetRecentAsync(int count = 100, CancellationToken cancellationToken = default);
}
