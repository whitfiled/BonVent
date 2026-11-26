namespace Contracts.Auth.Response;

public sealed record StaffResponse(Guid Id, string FirstName, string LastName, string Email, string Role, bool IsActive);
