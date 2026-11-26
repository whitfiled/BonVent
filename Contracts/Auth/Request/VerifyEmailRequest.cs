namespace Contracts.Auth.Request;

public sealed record VerifyEmailRequest(Guid UserId, string Token);
