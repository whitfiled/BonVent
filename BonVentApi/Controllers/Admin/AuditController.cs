using Application.Common.Interfaces;
using Contracts.Audit;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BonVentApi.Controllers.Admin;

[Tags("Audit")]
public sealed class AuditController(IAuditService audit) : AdminControllerBase
{
    [HttpGet]
    [Authorize(Roles = Roles.Owner + "," + Roles.Manager)]
    public async Task<ActionResult<IReadOnlyList<AuditEventResponse>>> Get([FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken cancellationToken = default)
    {
        if (skip < 0 || take is < 1 or > 100) return BadRequest("take must be between 1 and 100.");
        return Ok(await audit.GetRecentAsync(skip, take, cancellationToken));
    }
}
