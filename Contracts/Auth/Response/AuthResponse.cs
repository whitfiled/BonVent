namespace Contracts.Auth.Response;

public record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc);