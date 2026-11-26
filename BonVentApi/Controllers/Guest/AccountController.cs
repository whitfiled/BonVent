using Application.Common.Interfaces;
using Contracts.Auth.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BonVentApi.Controllers.Guest;

[ApiController]
[Authorize]
[Route("api/v1/account")]
[Tags("Account")]
internal sealed class AccountController(ICurrentUser currentUser, IAuthService auth) : ControllerBase
{
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId) return Forbid();
        await auth.ChangePasswordAsync(userId, request, cancellationToken);
        return NoContent();
    }
}
