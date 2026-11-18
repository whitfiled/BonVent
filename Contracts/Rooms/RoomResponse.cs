// Contracts/Rooms/RoomResponse.cs
namespace BonVent.Contracts.Rooms;

public record RoomResponse(
    Guid Id,
    string Number,
    int Floor,
    Guid RoomTypeId,
    string RoomTypeName,
    int Capacity,
    decimal BaseRate,
    bool IsOutOfService);