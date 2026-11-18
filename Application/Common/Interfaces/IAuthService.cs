using Contracts.Auth.Request;
using Contracts.Auth.Response;

namespace Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterGuestAsync(RegisterGuestRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken);
    Task CreateStaffAsync(CreateStaffRequest request, CancellationToken cancellationToken);
}
