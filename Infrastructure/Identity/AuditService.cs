using Application.Common.Interfaces;
using Contracts.Audit;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Identity;

internal sealed class AuditService(AuthDbContext db) : IAuditService
{
    public async Task RecordAsync(Guid? actorUserId, string action, string resourceType, string? resourceId, string? metadata, CancellationToken cancellationToken)
    {
        db.AuditEvents.Add(new AuditEvent
        {
            ActorUserId = actorUserId,
            Action = action,
            ResourceType = resourceType,
            ResourceId = resourceId,
            Metadata = metadata
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditEventResponse>> GetRecentAsync(int skip, int take, CancellationToken cancellationToken) =>
        await db.AuditEvents.AsNoTracking().OrderByDescending(item => item.CreatedAtUtc).Skip(skip).Take(take)
            .Select(item => new AuditEventResponse(item.Id, item.ActorUserId, item.Action, item.ResourceType, item.ResourceId, item.CreatedAtUtc, item.Metadata))
            .ToListAsync(cancellationToken);
}
