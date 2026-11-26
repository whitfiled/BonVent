using Contracts.Auth.Request;
using Contracts.Auth.Response;

namespace Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterGuestAsync(RegisterGuestRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken);
    Task CreateStaffAsync(CreateStaffRequest request, CancellationToken cancellationToken);
    Task BootstrapOwnerAsync(BootstrapOwnerRequest request, string setupKey, CancellationToken cancellationToken);
    Task<IReadOnlyList<StaffResponse>> GetStaffAsync(CancellationToken cancellationToken);
    Task<StaffResponse> UpdateStaffAsync(Guid id, UpdateStaffRequest request, CancellationToken cancellationToken);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken);
    Task RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken);
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken);
    Task VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken);
}
