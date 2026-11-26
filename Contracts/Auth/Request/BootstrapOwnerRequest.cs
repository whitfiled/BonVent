namespace Contracts.Auth.Request;

public sealed record BootstrapOwnerRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password);
