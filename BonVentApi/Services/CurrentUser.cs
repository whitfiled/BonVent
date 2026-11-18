using System.Security.Claims;
using Application.Common.Interfaces;

namespace BonVentApi.Services;

internal sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public Guid? UserId => ReadGuidClaim(ClaimTypes.NameIdentifier);
    public Guid? GuestId => ReadGuidClaim("guest_id");
    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;

    private Guid? ReadGuidClaim(string claimType)
    {
        return Guid.TryParse(Principal?.FindFirstValue(claimType), out var id) ? id : null;
    }
}
