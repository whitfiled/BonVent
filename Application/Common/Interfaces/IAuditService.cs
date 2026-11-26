using Contracts.Audit;

namespace Application.Common.Interfaces;

public interface IAuditService
{
    Task RecordAsync(Guid? actorUserId, string action, string resourceType, string? resourceId, string? metadata, CancellationToken cancellationToken);
    Task<IReadOnlyList<AuditEventResponse>> GetRecentAsync(int skip, int take, CancellationToken cancellationToken);
}
