using BonVent.Contracts.Rooms;
using System.ComponentModel.DataAnnotations;

namespace BonVentPublicSite.Models;

public class ReservationViewModel
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Phone { get; set; } = string.Empty;
    [Required] public string Address { get; set; } = string.Empty;
    [Required] public Guid? RoomId { get; set; }
    [Required, DataType(DataType.Date)] public DateOnly CheckIn { get; set; }
    [Required, DataType(DataType.Date)] public DateOnly CheckOut { get; set; }
    [Range(1, 20)] public int Guests { get; set; } = 1;
    public Guid IdempotencyKey { get; set; } = Guid.NewGuid();
    public IReadOnlyList<RoomResponse> AvailableRooms { get; set; } = [];
}
