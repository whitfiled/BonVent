namespace BonVent.Contracts.Rooms;

public sealed record CreateRoomTypeRequest(string Name, int Capacity, decimal BaseRate);
public sealed record UpdateRoomTypeRequest(string Name, int Capacity, decimal BaseRate);
public sealed record CreateRoomRequest(string Number, int Floor, Guid RoomTypeId);
public sealed record UpdateRoomRequest(string Number, int Floor, Guid RoomTypeId, bool IsOutOfService);
