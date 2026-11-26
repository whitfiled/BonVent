namespace Contracts.Audit;

public sealed record AuditEventResponse(
    Guid Id,
    Guid? ActorUserId,
    string Action,
    string ResourceType,
    string? ResourceId,
    DateTime CreatedAtUtc,
    string? Metadata);
