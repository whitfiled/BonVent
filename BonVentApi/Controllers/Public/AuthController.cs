using Application.Common.Interfaces;
using Contracts.Auth.Request;
using Contracts.Auth.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BonVentApi.Controllers.Public;

[ApiController]
[AllowAnonymous]
[Route("api/v1/auth")]
[Tags("Auth")]
public sealed class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterGuestRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await auth.RegisterGuestAsync(request, cancellationToken));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await auth.LoginAsync(request, cancellationToken));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(
        RefreshRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await auth.RefreshAsync(request, cancellationToken));
    }
}
