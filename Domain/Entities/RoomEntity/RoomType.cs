using Domain.Entities.Common;

namespace Domain.Entities.RoomEntity;

internal class RoomType : Entity
{
    public string Name { get; private set; } = default!;
    public int Capacity { get; private set; }
    public decimal BaseRate { get; private set; }

    private RoomType() { }

    public RoomType(string name, int capacity, decimal baseRate)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.");
        if (capacity <= 0) throw new ArgumentException("Capacity must be positive.");
        if (baseRate < 0) throw new ArgumentException("Rate cannot be negative.");

        Name = name;
        Capacity = capacity;
        BaseRate = baseRate;
    }
}
