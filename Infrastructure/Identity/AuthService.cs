using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Contracts.Auth.Request;
using Contracts.Auth.Response;
using Domain.Entities.Guests;
using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Identity;

internal sealed class AuthService(
    UserManager<ApplicationUser> users,
    AuthDbContext identityDb,
    HotelDbContext hotelDb,
    IJwtTokenService tokens,
    IOptions<JwtOptions> options,
    IOptions<BootstrapOptions> bootstrapOptions,
    RoleManager<IdentityRole<Guid>> roles,
    IAuditService audit,
    IEmailService email,
    ICurrentUser currentUser,
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
        hotelDb.Guests.Add(guest);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ApplicationUser? user = null;
        try
        {
            user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                GuestId = guest.Id
            };

            var createResult = await users.CreateAsync(user, request.Password);
            if (!createResult.Succeeded) throw new ArgumentException(FormatIdentityErrors(createResult));

            var roleResult = await users.AddToRoleAsync(user, Roles.Guest);
            if (!roleResult.Succeeded) throw new InvalidOperationException(FormatIdentityErrors(roleResult));

            var confirmationToken = await users.GenerateEmailConfirmationTokenAsync(user);
            await email.QueueAsync(user.Email!, "Confirm your BonVent account", $"UserId: {user.Id}\nToken: {confirmationToken}", cancellationToken);

            return await IssueTokensAsync(user, cancellationToken);
        }
        catch
        {
            if (user?.Id is not null && user.Id != Guid.Empty)
            {
                await users.DeleteAsync(user);
            }

            hotelDb.Guests.Remove(guest);
            await unitOfWork.SaveChangesAsync(CancellationToken.None);
            throw;
        }
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

        return await identityDb.ExecuteInSerializableTransactionAsync(async () =>
        {

            var storedToken = await identityDb.RefreshTokens
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
        await audit.RecordAsync(currentUser.UserId, "staff.created", "user", user.Id.ToString(), request.Role, cancellationToken);
    }

    public async Task BootstrapOwnerAsync(BootstrapOwnerRequest request, string setupKey, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var configuredKey = bootstrapOptions.Value.SetupKey;
        if (string.IsNullOrWhiteSpace(configuredKey)
            || !CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(configuredKey),
                Encoding.UTF8.GetBytes(setupKey ?? string.Empty)))
        {
            throw new UnauthorizedAccessException("Invalid bootstrap credentials.");
        }

        if (await users.Users.AnyAsync(cancellationToken))
        {
            throw new ConflictException("Owner bootstrap is only available before the first user is created.");
        }

        foreach (var role in Roles.All)
        {
            if (!await roles.RoleExistsAsync(role))
            {
                var result = await roles.CreateAsync(new IdentityRole<Guid>(role));
                if (!result.Succeeded) throw new InvalidOperationException(FormatIdentityErrors(result));
            }
        }

        var owner = new ApplicationUser
        {
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            EmailConfirmed = true,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim()
        };

        var createResult = await users.CreateAsync(owner, request.Password);
        if (!createResult.Succeeded) throw new ArgumentException(FormatIdentityErrors(createResult));

        var roleResult = await users.AddToRoleAsync(owner, Roles.Owner);
        if (!roleResult.Succeeded) throw new InvalidOperationException(FormatIdentityErrors(roleResult));
        await audit.RecordAsync(null, "owner.bootstrapped", "user", owner.Id.ToString(), null, cancellationToken);
    }

    public async Task<IReadOnlyList<StaffResponse>> GetStaffAsync(CancellationToken cancellationToken)
    {
        var staff = new List<StaffResponse>();
        foreach (var user in await users.Users.AsNoTracking().OrderBy(user => user.LastName).ToListAsync(cancellationToken))
        {
            var role = (await users.GetRolesAsync(user)).FirstOrDefault(role => role is Roles.Owner or Roles.Manager or Roles.Receptionist);
            if (role is not null) staff.Add(new StaffResponse(user.Id, user.FirstName, user.LastName, user.Email ?? string.Empty, role, user.IsActive));
        }
        return staff;
    }

    public async Task<StaffResponse> UpdateStaffAsync(Guid id, UpdateStaffRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Role is not (Roles.Manager or Roles.Receptionist)) throw new ArgumentException("Role must be Manager or Receptionist.", nameof(request));
        var user = await users.FindByIdAsync(id.ToString()) ?? throw new NotFoundException("Staff member not found.");
        var currentRoles = await users.GetRolesAsync(user);
        if (currentRoles.Contains(Roles.Owner)) throw new UnauthorizedAccessException("The Owner account cannot be edited here.");
        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.IsActive = request.IsActive;
        var update = await users.UpdateAsync(user);
        if (!update.Succeeded) throw new ArgumentException(FormatIdentityErrors(update));
        await users.RemoveFromRolesAsync(user, currentRoles);
        var roleResult = await users.AddToRoleAsync(user, request.Role);
        if (!roleResult.Succeeded) throw new InvalidOperationException(FormatIdentityErrors(roleResult));
        await audit.RecordAsync(currentUser.UserId, "staff.updated", "user", user.Id.ToString(), request.Role, cancellationToken);
        return new StaffResponse(user.Id, user.FirstName, user.LastName, user.Email ?? string.Empty, request.Role, user.IsActive);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var user = await users.FindByIdAsync(userId.ToString()) ?? throw new NotFoundException("User not found.");
        var result = await users.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded) throw new ArgumentException(FormatIdentityErrors(result));
        await audit.RecordAsync(userId, "account.password_changed", "user", userId.ToString(), null, cancellationToken);
    }

    public async Task RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var user = await users.FindByEmailAsync(request.Email.Trim());
        if (user is null || !user.IsActive) return;
        var token = await users.GeneratePasswordResetTokenAsync(user);
        await email.QueueAsync(user.Email!, "Reset your BonVent password", $"UserId: {user.Id}\nToken: {token}", cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var user = await users.FindByEmailAsync(request.Email.Trim()) ?? throw new ArgumentException("Invalid password reset request.");
        var result = await users.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded) throw new ArgumentException(FormatIdentityErrors(result));
        await audit.RecordAsync(user.Id, "account.password_reset", "user", user.Id.ToString(), null, cancellationToken);
    }

    public async Task VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var user = await users.FindByIdAsync(request.UserId.ToString()) ?? throw new NotFoundException("User not found.");
        var result = await users.ConfirmEmailAsync(user, request.Token);
        if (!result.Succeeded) throw new ArgumentException(FormatIdentityErrors(result));
    }

    private async Task<AuthResponse> IssueTokensAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var roles = await users.GetRolesAsync(user);

        var accessToken = tokens.CreateAccessToken(user, roles);

        var (rawToken, tokenHash) = tokens.CreateRefreshToken();

        var jwtOptions = options.Value;

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(jwtOptions.AccessTokenMinutes);

        identityDb.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(jwtOptions.RefreshTokenDays)
        });

        await identityDb.SaveChangesAsync(cancellationToken);

        return new AuthResponse(accessToken, rawToken, expiresAtUtc);
    }

    private static string FormatIdentityErrors(IdentityResult result)
    {
        return string.Join(" ", result.Errors.Select(error => error.Description));
    }
}
