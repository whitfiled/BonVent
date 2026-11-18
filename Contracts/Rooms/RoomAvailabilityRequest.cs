// Contracts/Rooms/RoomAvailabilityRequest.cs
namespace BonVent.Contracts.Rooms;

public record RoomAvailabilityRequest(
    DateOnly CheckIn,
    DateOnly CheckOut,
    int? Guests = null,
    Guid? RoomTypeId = null);