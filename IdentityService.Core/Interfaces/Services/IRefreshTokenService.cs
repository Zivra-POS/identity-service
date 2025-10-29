using System.Threading;
using System.Threading.Tasks;
using IdentityService.Core.Entities;

namespace IdentityService.Core.Interfaces.Services;

public interface IRefreshTokenService
{
    Task<string> GenerateAndStoreRefreshTokenAsync(Guid userId, Guid accessTokenId,  CancellationToken ct = default);
    Task<Guid?> ValidateRefreshTokenAsync(Guid userId, string token, CancellationToken ct = default);
    Task RevokeRefreshTokenAsync(Guid id, string? revokedByIp = null, CancellationToken ct = default);
    Task<(User user, string newRefreshToken)?> RotateRefreshTokenAsync(string existingRawToken, string? deviceId = null, CancellationToken ct = default);
}
