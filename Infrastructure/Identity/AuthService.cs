using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Contracts.Auth.Request;
using Contracts.Auth.Response;
using Domain.Entities.Guests;
using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Identity;

internal sealed class AuthService(
    UserManager<ApplicationUser> users,
    HotelDbContext db,
    IJwtTokenService tokens,
    IOptions<JwtOptions> options,
    IUnitOfWork unitOfWork,
    RefreshTokenConcurrencyGuard refreshTokenConcurrencyGuard) : IAuthService
{
    public async Task<AuthResponse> RegisterGuestAsync(RegisterGuestRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await users.FindByEmailAsync(request.Email) is not null)
        {
            throw new ConflictException("Email is already registered.");
        }

        var guest = new Guest(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone ?? string.Empty,
            request.Address ?? string.Empty);
        db.Guests.Add(guest);

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            GuestId = guest.Id
        };

        var createResult = await users.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            db.Entry(guest).State = EntityState.Detached;
            throw new ArgumentException(FormatIdentityErrors(createResult));
        }

        var roleResult = await users.AddToRoleAsync(user, Roles.Guest);
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(FormatIdentityErrors(roleResult));
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await users.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive || await users.IsLockedOutAsync(user)
            || !await users.CheckPasswordAsync(user, request.Password))
        {
            if (user is not null && user.IsActive)
            {
                await users.AccessFailedAsync(user);
            }

            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        await users.ResetAccessFailedCountAsync(user);
        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tokenHash = tokens.Hash(request.RefreshToken);
        await using var tokenLock = await refreshTokenConcurrencyGuard.AcquireAsync(tokenHash, cancellationToken);

        return await unitOfWork.ExecuteInSerializableTransactionAsync(async () =>
        {

            var storedToken = await db.RefreshTokens
                .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

            if (storedToken is null || !storedToken.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            var user = await users.FindByIdAsync(storedToken.UserId.ToString());

            if (user is null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            storedToken.RevokedAtUtc = DateTime.UtcNow;

            return await IssueTokensAsync(user, cancellationToken);

        }, cancellationToken);
    }

    public async Task CreateStaffAsync(CreateStaffRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Role is not (Roles.Manager or Roles.Receptionist))
        {
            throw new ArgumentException("Role must be Manager or Receptionist.", nameof(request));
        }

        if (await users.FindByEmailAsync(request.Email) is not null)
        {
            throw new ConflictException("Email is already registered.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var createResult = await users.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            throw new ArgumentException(FormatIdentityErrors(createResult));
        }

        var roleResult = await users.AddToRoleAsync(user, request.Role);

        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(FormatIdentityErrors(roleResult));
        }
    }

    private async Task<AuthResponse> IssueTokensAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var roles = await users.GetRolesAsync(user);

        var accessToken = tokens.CreateAccessToken(user, roles);

        var (rawToken, tokenHash) = tokens.CreateRefreshToken();

        var jwtOptions = options.Value;

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(jwtOptions.AccessTokenMinutes);

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(jwtOptions.RefreshTokenDays)
        });

        await db.SaveChangesAsync(cancellationToken);

        return new AuthResponse(accessToken, rawToken, expiresAtUtc);
    }

    private static string FormatIdentityErrors(IdentityResult result)
    {
        return string.Join(" ", result.Errors.Select(error => error.Description));
    }
}
