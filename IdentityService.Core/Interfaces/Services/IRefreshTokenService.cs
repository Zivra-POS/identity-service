using System.Threading;
using System.Threading.Tasks;
using IdentityService.Core.Entities;

namespace IdentityService.Core.Interfaces.Services;

public interface IRefreshTokenService
{
    /// <summary>
    /// Generate a new refresh token for a user and persist it. Returns the raw token value (not hashed).
    /// </summary>
    Task<string> GenerateAndStoreRefreshTokenAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Validates a refresh token (raw token) for a user and returns the corresponding RefreshToken entity id or null if invalid.
    /// </summary>
    Task<Guid?> ValidateRefreshTokenAsync(Guid userId, string token, CancellationToken ct = default);

    /// <summary>
    /// Revoke a refresh token (by id or token).
    /// </summary>
    Task RevokeRefreshTokenAsync(Guid id, string? revokedByIp = null, CancellationToken ct = default);

    /// <summary>
    /// Rotate an existing refresh token (raw). On success returns (User, newRawToken), otherwise null.
    /// </summary>
    Task<(User user, string newRefreshToken)?> RotateRefreshTokenAsync(string existingRawToken, string? deviceId = null, CancellationToken ct = default);
}
