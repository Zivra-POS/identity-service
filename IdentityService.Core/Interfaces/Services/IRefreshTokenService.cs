using System.Threading;
using System.Threading.Tasks;
using IdentityService.Core.Entities;

namespace IdentityService.Core.Interfaces.Services;

public interface IRefreshTokenService
{
    Task<RefreshToken?> GetByRefreshTokenHashAsync(string rawToken, CancellationToken ct = default);
    Task<string> GenerateAndStoreRefreshTokenAsync(Guid userId, Guid accessTokenId,  CancellationToken ct = default);
    Task<Guid?> ValidateRefreshTokenAsync(Guid userId, string token, CancellationToken ct = default);
    Task RevokeRefreshTokenAsync(Guid id, string? revokedByIp = null, CancellationToken ct = default);
    Task<string?> RotateRefreshTokenAsync(string existingRawToken, Guid accessTokenId, string? deviceId = null, CancellationToken ct = default);
    Task RevokeAllRefreshTokensForUserAsync(Guid userId, string? revokedByIp = null, CancellationToken ct = default);
}
