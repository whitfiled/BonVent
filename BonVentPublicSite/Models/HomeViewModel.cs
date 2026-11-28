using BonVent.Contracts.Rooms;

namespace BonVentPublicSite.Models;

public sealed class HomeViewModel
{
    public DateOnly CheckIn { get; init; }
    public DateOnly CheckOut { get; init; }
    public int Guest { get; init; }
    public IReadOnlyList<RoomResponse> Rooms { get; init; } = [];

}
