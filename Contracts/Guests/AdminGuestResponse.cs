namespace BonVent.Contracts.Guests;

public sealed record AdminGuestResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Address,
    bool IsDeleted);

public sealed record UpdateGuestRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Address);
