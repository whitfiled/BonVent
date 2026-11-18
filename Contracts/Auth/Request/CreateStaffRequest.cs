namespace Contracts.Auth.Request;

public record CreateStaffRequest(string FirstName, string LastName, string Email, string Password, string Role);
