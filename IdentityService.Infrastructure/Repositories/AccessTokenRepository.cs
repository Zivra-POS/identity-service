using IdentityService.Core.Entities;
using IdentityService.Core.Interfaces.Repositories;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ZivraFramework.EFCore.Repositories;

namespace IdentityService.Infrastructure.Repositories;

public class AccessTokenRepository : GenericRepository<AccessToken>, IAccessTokenRepository
{
    public AccessTokenRepository(IdentityDbContext ctx) : base(ctx)
    {
    }
    
    #region RevokeAsync
    public async Task RevokeAsync(Guid accessTokenId, string? revokedByIp = null, CancellationToken ct = default)
    {
        var token = await GetByIdAsync(accessTokenId, ct);
        if (token != null && token.Revoked == null)
        {
            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = revokedByIp;
            token.ModDate = DateTime.UtcNow;
            
            Update(token);
        }
    }
    #endregion
    
    #region IsRevokedAsync
    public async Task<bool> IsRevokedAsync(Guid accessTokenId, CancellationToken ct = default)
    {
        var token = await _set
            .Where(t => t.Id == accessTokenId)
            .Select(t => new { t.Revoked })
            .FirstOrDefaultAsync(ct);
            
        return token?.Revoked != null;
    }
    #endregion
    
    #region GetByIdWithUserAsync
    public async Task<AccessToken?> GetByIdWithUserAsync(Guid id, CancellationToken ct = default)
    {
        return await _set
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }
    #endregion
}