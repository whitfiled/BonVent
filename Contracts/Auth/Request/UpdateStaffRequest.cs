namespace Contracts.Auth.Request;

public sealed record UpdateStaffRequest(string FirstName, string LastName, string Role, bool IsActive);
