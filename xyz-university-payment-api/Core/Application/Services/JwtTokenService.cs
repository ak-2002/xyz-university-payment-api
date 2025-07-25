// Purpose: JWT Token Service implementation for authentication token management
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using xyz_university_payment_api.Core.Application.Interfaces;
using xyz_university_payment_api.Core.Domain.Entities;
using xyz_university_payment_api.Infrastructure.Data;
using xyz_university_payment_api.Core.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace xyz_university_payment_api.Core.Application.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly ILogger<JwtTokenService> _logger;

        public JwtTokenService(
            IConfiguration configuration,
            AppDbContext context,
            ILogger<JwtTokenService> logger)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        public async Task<string> GenerateAccessTokenAsync(User user)
        {
            try
            {
                // Get user roles and permissions directly from database
                var userRoles = await GetUserRolesFromDatabaseAsync(user.Username);
                var userPermissions = await GetUserPermissionsFromDatabaseAsync(user.Username);

                // Create claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("user_id", user.Id.ToString()),
                    new Claim("is_active", user.IsActive.ToString()),
                    new Claim("jti", Guid.NewGuid().ToString()), // JWT ID for token uniqueness
                    new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64), // Issued at
                    new Claim("scope", "xyz_api") // API scope
                };

                // Add roles to claims
                foreach (var role in userRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                    claims.Add(new Claim("role", role)); // Additional role claim for flexibility
                }

                // Add permissions to claims
                foreach (var permission in userPermissions)
                {
                    claims.Add(new Claim("permission", permission));
                }

                // Add custom claims for better authorization
                claims.Add(new Claim("user_type", user.IsActive ? "active" : "inactive"));
                claims.Add(new Claim("created_at", user.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")));

                // Create JWT token
                var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured. Please set the Jwt:Key configuration value.");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"] ?? "xyz-university",
                    audience: _configuration["Jwt:Audience"] ?? "xyz-api",
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(Convert.ToInt32(_configuration["Jwt:ExpiryInHours"] ?? "1")),
                    notBefore: DateTime.UtcNow,
                    signingCredentials: credentials
                );

                _logger.LogInformation("Generated access token for user {Username} with {RoleCount} roles and {PermissionCount} permissions",
                    user.Username, userRoles.Count(), userPermissions.Count());

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating access token for user {Username}", user.Username);
                throw;
            }
        }

        private async Task<IEnumerable<string>> GetUserRolesFromDatabaseAsync(string username)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !user.IsActive)
            {
                return Enumerable.Empty<string>();
            }

            return user.UserRoles
                .Where(ur => ur.Role.IsActive)
                .Select(ur => ur.Role.Name)
                .ToList();
        }

        private async Task<IEnumerable<string>> GetUserPermissionsFromDatabaseAsync(string username)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !user.IsActive)
            {
                return Enumerable.Empty<string>();
            }

            return user.UserRoles
                .Where(ur => ur.Role.IsActive)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Where(rp => rp.Permission.IsActive)
                .Select(rp => $"{rp.Permission.Resource}.{rp.Permission.Action}")
                .Distinct()
                .ToList();
        }

        public async Task<string> GenerateRefreshTokenAsync()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<bool> ValidateAccessTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured. Please set the Jwt:Key configuration value.");
                var key = Encoding.UTF8.GetBytes(jwtKey);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"] ?? "xyz-university",
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"] ?? "xyz-api",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return false;
            }
        }

        public async Task<bool> ValidateRefreshTokenAsync(string refreshToken)
        {
            // For now, we'll return true as a placeholder
            // In a real application, you would validate against the database
            return true;
        }

        public async Task<string> RefreshAccessTokenAsync(string refreshToken)
        {
            // For now, we'll throw an exception as this requires database implementation
            throw new NotImplementedException("Refresh token functionality requires database implementation");
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            // In a real application, you would:
            // 1. Find the refresh token in the database
            // 2. Mark it as revoked or delete it
            // 3. Return true if successful

            // For now, we'll return true as a placeholder
            return true;
        }

        public async Task<string?> GetUsernameFromTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                return jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error extracting username from token");
                return null;
            }
        }

        public async Task<IEnumerable<string>> GetRolesFromTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                return jwtToken.Claims
                    .Where(x => x.Type == ClaimTypes.Role)
                    .Select(x => x.Value)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error extracting roles from token");
                return Enumerable.Empty<string>();
            }
        }

        public async Task<IEnumerable<string>> GetPermissionsFromTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                return jwtToken.Claims
                    .Where(x => x.Type == "permission")
                    .Select(x => x.Value)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error extracting permissions from token");
                return Enumerable.Empty<string>();
            }
        }

        public async Task<DateTime> GetTokenExpirationAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                return jwtToken.ValidTo;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error extracting token expiration");
                return DateTime.MinValue;
            }
        }

        public async Task<bool> IsTokenExpiredAsync(string token)
        {
            try
            {
                var expiration = await GetTokenExpirationAsync(token);
                return expiration <= DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking token expiration");
                return true; // Assume expired if we can't check
            }
        }
    }
}