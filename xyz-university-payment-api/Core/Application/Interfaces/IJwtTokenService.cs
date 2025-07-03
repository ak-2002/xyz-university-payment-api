// Purpose: JWT Token Service interface for authentication token management
using xyz_university_payment_api.Core.Domain.Entities;

namespace xyz_university_payment_api.Core.Application.Interfaces
{
    public interface IJwtTokenService
    {
        // Token generation
        Task<string> GenerateAccessTokenAsync(User user);
        Task<string> GenerateRefreshTokenAsync();

        // Token validation
        Task<bool> ValidateAccessTokenAsync(string token);
        Task<bool> ValidateRefreshTokenAsync(string refreshToken);

        // Token refresh
        Task<string> RefreshAccessTokenAsync(string refreshToken);

        // Token revocation
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);

        // Claims extraction
        Task<string?> GetUsernameFromTokenAsync(string token);
        Task<IEnumerable<string>> GetRolesFromTokenAsync(string token);
        Task<IEnumerable<string>> GetPermissionsFromTokenAsync(string token);

        // Token information
        Task<DateTime> GetTokenExpirationAsync(string token);
        Task<bool> IsTokenExpiredAsync(string token);
    }
}