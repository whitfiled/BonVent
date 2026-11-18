namespace BonVent.Contracts.Guests;

public record GuestResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone
    );
