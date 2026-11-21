using BonVent.Contracts.Rooms;
using System.ComponentModel.DataAnnotations;

namespace BonVentPublicSite.Models;

public sealed class PublicReservationForm
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Phone { get; set; } = string.Empty;
    [Required] public string Address { get; set; } = string.Empty;
    [Required] public Guid? RoomId { get; set; }
    [Required, DataType(DataType.Date)] public DateOnly CheckIn { get; set; }
    [Required, DataType(DataType.Date)] public DateOnly CheckOut { get; set; }
    public IReadOnlyList<RoomResponse> AvailableRooms { get; set; } = [];
}
