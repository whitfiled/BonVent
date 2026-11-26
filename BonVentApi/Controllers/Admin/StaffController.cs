using Application.Common.Interfaces;
using Contracts.Auth.Request;
using Contracts.Auth.Response;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BonVentApi.Controllers.Admin;

[Authorize(Roles = Roles.Owner)]
[Tags("Staff")]
public sealed class StaffController(IAuthService auth) : AdminControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StaffResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await auth.GetStaffAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(CreateStaffRequest request, CancellationToken cancellationToken)
    {
        await auth.CreateStaffAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StaffResponse>> Update(Guid id, UpdateStaffRequest request, CancellationToken cancellationToken) =>
        Ok(await auth.UpdateStaffAsync(id, request, cancellationToken));
}
