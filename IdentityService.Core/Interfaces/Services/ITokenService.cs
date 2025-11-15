using IdentityService.Core.Entities;
using System.Threading;

namespace IdentityService.Core.Interfaces.Services;

public interface ITokenService
{
    Task<AccessToken> GenerateJwtToken(User user, IEnumerable<string?> roles);
    Task RevokeJwtTokenAsync(Guid accessTokenId);
    Task<bool> IsJwtTokenRevokedAsync(Guid accessTokenId);
    Task RevokeAllAccessTokensForUserAsync(Guid userId, string? revokedByIp = null, CancellationToken ct = default);
}