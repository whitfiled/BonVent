namespace MobileBonVent.Models;

internal class Room
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? RoomType { get; set; }
    public float PricePerNight { get; set; }
    public int Capacity { get; set; }
    public bool IsAvailable { get; set; }
    public string? ImageUrl { get; set; }
}
