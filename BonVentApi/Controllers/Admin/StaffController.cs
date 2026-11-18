using Application.Common.Interfaces;
using Contracts.Auth.Request;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BonVentApi.Controllers.Admin;

[Authorize(Roles = Roles.Owner)]
[Tags("Staff")]
public sealed class StaffController(IAuthService auth) : AdminControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateStaffRequest request, CancellationToken cancellationToken)
    {
        await auth.CreateStaffAsync(request, cancellationToken);
        return NoContent();
    }
}
