namespace Contracts.Auth.Request;

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
