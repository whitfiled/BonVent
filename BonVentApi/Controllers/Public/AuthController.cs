using Application.Common.Interfaces;
using Contracts.Auth.Request;
using Contracts.Auth.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BonVentApi.Controllers.Public;

[ApiController]
[AllowAnonymous]
[Route("api/v1/auth")]
[Tags("Auth")]
[EnableRateLimiting("Auth")]
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

    [HttpPost("/api/v1/setup/owner")]
    [EnableRateLimiting("Bootstrap")]
    public async Task<IActionResult> BootstrapOwner(
        BootstrapOwnerRequest request,
        [FromHeader(Name = "X-Bootstrap-Key")] string? setupKey,
        CancellationToken cancellationToken)
    {
        await auth.BootstrapOwnerAsync(request, setupKey ?? string.Empty, cancellationToken);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await auth.RequestPasswordResetAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await auth.ResetPasswordAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        await auth.VerifyEmailAsync(request, cancellationToken);
        return NoContent();
    }
}
