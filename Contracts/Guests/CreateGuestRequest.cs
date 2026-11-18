namespace BonVent.Contracts.Guests;

public record CreateGuestRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone);
