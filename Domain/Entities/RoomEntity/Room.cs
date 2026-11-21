using Domain.Entities.Common;

namespace Domain.Entities.RoomEntity;

internal class Room : AuditableEntity, IAggregateRoot
{
    public string Number { get; private set; } = default!;
    public int Floor { get; private set; }
    public bool IsOutOfService { get; private set; }
    public bool IsAvailable => !IsOutOfService;
    public Guid RoomTypeId { get; private set; }
    public RoomType RoomType { get; private set; } = default!;
    public ICollection<RoomImage> Images { get; private set; } = new List<RoomImage>();

    private Room() { }

    public Room(string number, int floor, Guid roomTypeId)
    {
        if (string.IsNullOrWhiteSpace(number)) throw new ArgumentException("Room number is required.");
        Number = number;
        Floor = floor;
        RoomTypeId = roomTypeId;
    }

    public void SetOutOfService(bool value) => IsOutOfService = value;
}
