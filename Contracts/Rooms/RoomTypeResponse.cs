namespace BonVent.Contracts.Rooms;

public sealed record RoomTypeResponse(Guid Id, string Name, int Capacity, decimal BaseRate);
