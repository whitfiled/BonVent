namespace Contracts.Auth.Request;

public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);
