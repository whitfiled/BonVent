namespace BonVent.Contracts.Rooms;

public sealed record RoomImageResponse(
    Guid Id,
    string Url,
    string? AltText,
    bool IsPrimary,
    int SortOrder);
