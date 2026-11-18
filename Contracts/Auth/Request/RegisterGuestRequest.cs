namespace Contracts.Auth.Request;

public sealed record RegisterGuestRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? Phone,
    string? Address = null);
