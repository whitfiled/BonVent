namespace Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }
    Guid? GuestId { get; }
    bool IsInRole(string role);
}
