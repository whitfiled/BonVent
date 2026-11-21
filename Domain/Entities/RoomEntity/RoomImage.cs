using Domain.Entities.Common;

namespace Domain.Entities.RoomEntity;

internal sealed class RoomImage : Entity
{
    public Guid RoomId { get; private set; }
    public Room Room { get; private set; } = default!;
    public string BlobName { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public string? AltText { get; private set; }
    public bool IsPrimary { get; private set; }
    public int SortOrder { get; private set; }

    private RoomImage() { }

    public RoomImage(
        Guid roomId,
        string blobName,
        string contentType,
        string? altText,
        bool isPrimary,
        int sortOrder)
    {
        if (roomId == Guid.Empty) throw new ArgumentException("Room is required.");
        if (string.IsNullOrWhiteSpace(blobName)) throw new ArgumentException("Blob name is required.");
        if (string.IsNullOrWhiteSpace(contentType)) throw new ArgumentException("Content type is required.");

        RoomId = roomId;
        BlobName = blobName;
        ContentType = contentType;
        AltText = altText;
        IsPrimary = isPrimary;
        SortOrder = sortOrder;
    }
}
