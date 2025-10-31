using IdentityService.Core.Entities;
using ZivraFramework.Core.Interfaces;

namespace IdentityService.Core.Interfaces.Repositories;

public interface IAccessTokenRepository : IGenericRepository<AccessToken>
{
    Task RevokeAsync(Guid accessTokenId, string? revokedByIp = null, CancellationToken ct = default);
    Task<bool> IsRevokedAsync(Guid accessTokenId, CancellationToken ct = default);
    Task<AccessToken?> GetByIdWithUserAsync(Guid id, CancellationToken ct = default);
}