using BonVent.Contracts.Rooms;
using Domain.IRepositories;

namespace Application.Rooms.Queries.GetAvailableRooms;

public sealed record GetAvailableRoomsQuery(DateOnly CheckIn, DateOnly CheckOut, int? Guests, Guid? RoomTypeId);

public interface IGetAvailableRooms
{
    Task<IReadOnlyList<RoomResponse>> HandleAsync(GetAvailableRoomsQuery query, CancellationToken cancellationToken);
}

internal sealed class GetAvailableRoomsHandler(IRoomRepository rooms) : IGetAvailableRooms
{
    private readonly IRoomRepository _rooms = rooms;

    public async Task<IReadOnlyList<RoomResponse>> HandleAsync(GetAvailableRoomsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.CheckOut <= query.CheckIn)
        {
            throw new ArgumentException("Check-out must be after check-in.", nameof(query));
        }

        if (query.CheckIn < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException("Check-in cannot be in the past.", nameof(query));
        }

        if (query.Guests is <= 0)
        {
            throw new ArgumentException("Guests must be greater than zero.", nameof(query));
        }

        var rooms = await _rooms.GetAvailableAsync(
            query.CheckIn,
            query.CheckOut,
            query.Guests,
            query.RoomTypeId,
            cancellationToken);

        return rooms.Select(room => new RoomResponse(
                room.Id,
                room.Number,
                room.Floor,
                room.RoomTypeId,
                room.RoomType.Name,
                room.RoomType.Capacity,
                room.RoomType.BaseRate,
                room.IsOutOfService,
                room.Images.OrderBy(image => image.SortOrder)
                    .Select(image => new RoomImageResponse(
                        image.Id,
                        $"/api/v1/rooms/{room.Id}/images/{image.Id}",
                        image.AltText,
                        image.IsPrimary,
                        image.SortOrder))
                    .ToArray()))
            .ToArray();
    }
}
