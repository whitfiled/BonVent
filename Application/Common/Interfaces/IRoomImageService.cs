using BonVent.Contracts.Rooms;

namespace Application.Common.Interfaces;

public sealed record RoomImageUpload(Stream Content, string FileName, string ContentType, string? AltText);

public sealed record StoredRoomImage(Stream Content, string ContentType);


public interface IRoomImageService
{
    Task<RoomImageResponse> AddAsync(Guid roomId, RoomImageUpload upload, CancellationToken cancellationToken);

    Task<StoredRoomImage> GetAsync(Guid roomId, Guid imageId, CancellationToken cancellationToken);

    Task DeleteAsync(Guid roomId, Guid imageId, CancellationToken cancellationToken);
    Task SetPrimaryAsync(Guid roomId, Guid imageId, CancellationToken cancellationToken);

}
