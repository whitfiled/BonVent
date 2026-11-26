using Application.Common.Exceptions;
using Application.Common.Interfaces;
using BonVent.Contracts.Guests;
using BonVent.Contracts.Rooms;
using Domain.Entities.RoomEntity;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

internal sealed class HotelManagementService(HotelDbContext db, IAuditService audit, ICurrentUser currentUser) : IHotelManagementService
{
    public async Task<IReadOnlyList<RoomTypeResponse>> GetRoomTypesAsync(CancellationToken cancellationToken) =>
        await db.RoomTypes.AsNoTracking().OrderBy(type => type.Name)
            .Select(type => new RoomTypeResponse(type.Id, type.Name, type.Capacity, type.BaseRate))
            .ToListAsync(cancellationToken);

    public async Task<RoomTypeResponse> CreateRoomTypeAsync(CreateRoomTypeRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var type = new RoomType(request.Name.Trim(), request.Capacity, request.BaseRate);
        db.RoomTypes.Add(type);
        await db.SaveChangesAsync(cancellationToken);
        await audit.RecordAsync(currentUser.UserId, "room_type.created", "room_type", type.Id.ToString(), null, cancellationToken);
        return new RoomTypeResponse(type.Id, type.Name, type.Capacity, type.BaseRate);
    }

    public async Task<RoomTypeResponse> UpdateRoomTypeAsync(Guid id, UpdateRoomTypeRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var type = await db.RoomTypes.SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new NotFoundException("Room type not found.");
        type.Update(request.Name, request.Capacity, request.BaseRate);
        await db.SaveChangesAsync(cancellationToken);
        await audit.RecordAsync(currentUser.UserId, "room_type.updated", "room_type", type.Id.ToString(), null, cancellationToken);
        return new RoomTypeResponse(type.Id, type.Name, type.Capacity, type.BaseRate);
    }

    public async Task<RoomResponse> CreateRoomAsync(CreateRoomRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!await db.RoomTypes.AnyAsync(type => type.Id == request.RoomTypeId, cancellationToken))
            throw new NotFoundException("Room type not found.");
        var room = new Room(request.Number, request.Floor, request.RoomTypeId);
        db.Rooms.Add(room);
        await db.SaveChangesAsync(cancellationToken);
        await db.Entry(room).Reference(item => item.RoomType).LoadAsync(cancellationToken);
        await audit.RecordAsync(currentUser.UserId, "room.created", "room", room.Id.ToString(), null, cancellationToken);
        return MapRoom(room);
    }

    public async Task<RoomResponse> UpdateRoomAsync(Guid id, UpdateRoomRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var room = await db.Rooms.Include(item => item.RoomType).Include(item => item.Images)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new NotFoundException("Room not found.");
        if (!await db.RoomTypes.AnyAsync(type => type.Id == request.RoomTypeId, cancellationToken))
            throw new NotFoundException("Room type not found.");
        room.Update(request.Number, request.Floor, request.RoomTypeId);
        room.SetOutOfService(request.IsOutOfService);
        await db.SaveChangesAsync(cancellationToken);
        await db.Entry(room).Reference(item => item.RoomType).LoadAsync(cancellationToken);
        await audit.RecordAsync(currentUser.UserId, "room.updated", "room", room.Id.ToString(), null, cancellationToken);
        return MapRoom(room);
    }

    public async Task<IReadOnlyList<AdminGuestResponse>> GetGuestsAsync(int skip, int take, CancellationToken cancellationToken) =>
        await db.Guests.IgnoreQueryFilters().AsNoTracking().OrderBy(guest => guest.LastName).ThenBy(guest => guest.FirstName)
            .Skip(skip).Take(take)
            .Select(guest => new AdminGuestResponse(guest.Id, guest.FirstName, guest.LastName, guest.Email, guest.Phone, guest.Address, guest.IsDeleted))
            .ToListAsync(cancellationToken);

    public async Task<AdminGuestResponse> GetGuestAsync(Guid id, CancellationToken cancellationToken)
    {
        var guest = await db.Guests.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new NotFoundException("Guest not found.");
        return MapGuest(guest);
    }

    public async Task<AdminGuestResponse> UpdateGuestAsync(Guid id, UpdateGuestRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var guest = await db.Guests.IgnoreQueryFilters().SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new NotFoundException("Guest not found.");
        guest.Update(request.FirstName, request.LastName, request.Email, request.Phone, request.Address);
        await db.SaveChangesAsync(cancellationToken);
        await audit.RecordAsync(currentUser.UserId, "guest.updated", "guest", guest.Id.ToString(), null, cancellationToken);
        return MapGuest(guest);
    }

    public async Task DeactivateGuestAsync(Guid id, CancellationToken cancellationToken)
    {
        var guest = await db.Guests.SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new NotFoundException("Guest not found.");
        guest.Deactivate();
        await db.SaveChangesAsync(cancellationToken);
        await audit.RecordAsync(currentUser.UserId, "guest.deactivated", "guest", guest.Id.ToString(), null, cancellationToken);
    }

    private static RoomResponse MapRoom(Room room) => new(
        room.Id, room.Number, room.Floor, room.RoomTypeId, room.RoomType.Name, room.RoomType.Capacity,
        room.RoomType.BaseRate, room.IsOutOfService,
        room.Images.OrderBy(image => image.SortOrder)
            .Select(image => new RoomImageResponse(image.Id, image.SourceUrl ?? $"/api/v1/rooms/{room.Id}/images/{image.Id}", image.AltText, image.IsPrimary, image.SortOrder)).ToArray());

    private static AdminGuestResponse MapGuest(Domain.Entities.Guests.Guest guest) =>
        new(guest.Id, guest.FirstName, guest.LastName, guest.Email, guest.Phone, guest.Address, guest.IsDeleted);
}
